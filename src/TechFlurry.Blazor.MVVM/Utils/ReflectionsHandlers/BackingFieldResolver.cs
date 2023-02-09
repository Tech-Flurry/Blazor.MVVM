using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace TechFlurry.Blazor.MVVM.Utils.ReflectionsHandlers;
public static class BackingFieldResolver
{

    class FieldPattern : ILPattern
    {

        public static object FieldKey = new object();

        ILPattern pattern;

        public FieldPattern(ILPattern pattern)
        {
            this.pattern = pattern;
        }

        public override void Match(MatchContext context)
        {
            pattern.Match(context);
            if (!context.success)
                return;

            var match = GetLastMatchingInstruction(context);
            var field = (FieldInfo)match.Operand;
            context.AddData(FieldKey, field);
        }
    }

    static ILPattern Field(OpCode opcode)
    {
        return new FieldPattern(ILPattern.OpCode(opcode));
    }

    static ILPattern GetterPattern =
        ILPattern.Sequence(
            ILPattern.Optional(OpCodes.Nop),
            ILPattern.Either(
                Field(OpCodes.Ldsfld),
                ILPattern.Sequence(
                    ILPattern.OpCode(OpCodes.Ldarg_0),
                    Field(OpCodes.Ldfld))),
            ILPattern.Optional(
                ILPattern.Sequence(
                    ILPattern.OpCode(OpCodes.Stloc_0),
                    ILPattern.OpCode(OpCodes.Br_S),
                    ILPattern.OpCode(OpCodes.Ldloc_0))),
            ILPattern.Optional(ILPattern.OpCode(OpCodes.Br_S)),
            ILPattern.OpCode(OpCodes.Ret));

    static ILPattern SetterPattern =
        ILPattern.Sequence(
            ILPattern.Optional(OpCodes.Nop),
            ILPattern.OpCode(OpCodes.Ldarg_0),
            ILPattern.Either(
                Field(OpCodes.Stsfld),
                ILPattern.Sequence(
                    ILPattern.OpCode(OpCodes.Ldarg_1),
                    Field(OpCodes.Stfld))),
            ILPattern.OpCode(OpCodes.Ret));

    static FieldInfo GetBackingField(MethodInfo method, ILPattern pattern)
    {
        var result = ILPattern.Match(method, pattern);
        if (!result.success)
            throw new ArgumentException();

        object value;
        if (!result.TryGetData(FieldPattern.FieldKey, out value))
            throw new InvalidOperationException();

        return (FieldInfo)value;
    }

    public static FieldInfo GetBackingField(this PropertyInfo self)
    {
        if (self == null)
            throw new ArgumentNullException("self");

        var getter = self.GetGetMethod(true);
        if (getter != null)
            return GetBackingField(getter, GetterPattern);

        var setter = self.GetSetMethod(true);
        if (setter != null)
            return GetBackingField(setter, SetterPattern);

        throw new ArgumentException();
    }
}

public abstract class ILPattern
{

    public static ILPattern Optional(OpCode opcode)
    {
        return Optional(OpCode(opcode));
    }

    public static ILPattern Optional(params OpCode[ ] opcodes)
    {
        return Optional(Sequence(opcodes.Select(opcode => OpCode(opcode)).ToArray()));
    }

    public static ILPattern Optional(ILPattern pattern)
    {
        return new OptionalPattern(pattern);
    }

    class OptionalPattern : ILPattern
    {

        ILPattern pattern;

        public OptionalPattern(ILPattern optional)
        {
            this.pattern = optional;
        }

        public override void Match(MatchContext context)
        {
            pattern.TryMatch(context);
        }
    }

    public static ILPattern Sequence(params ILPattern[ ] patterns)
    {
        return new SequencePattern(patterns);
    }

    class SequencePattern : ILPattern
    {

        ILPattern[ ] patterns;

        public SequencePattern(ILPattern[ ] patterns)
        {
            this.patterns = patterns;
        }

        public override void Match(MatchContext context)
        {
            foreach (var pattern in patterns)
            {
                pattern.Match(context);

                if (!context.success)
                    break;
            }
        }
    }

    public static ILPattern OpCode(OpCode opcode)
    {
        return new OpCodePattern(opcode);
    }

    class OpCodePattern : ILPattern
    {

        OpCode opcode;

        public OpCodePattern(OpCode opcode)
        {
            this.opcode = opcode;
        }

        public override void Match(MatchContext context)
        {
            if (context.instruction == null)
            {
                context.success = false;
                return;
            }

            context.success = context.instruction.OpCode == opcode;
            context.Advance();
        }
    }

    public static ILPattern Either(ILPattern a, ILPattern b)
    {
        return new EitherPattern(a, b);
    }

    class EitherPattern : ILPattern
    {

        ILPattern a;
        ILPattern b;

        public EitherPattern(ILPattern a, ILPattern b)
        {
            this.a = a;
            this.b = b;
        }

        public override void Match(MatchContext context)
        {
            if (!a.TryMatch(context))
                b.Match(context);
        }
    }

    public abstract void Match(MatchContext context);

    protected static Instruction GetLastMatchingInstruction(MatchContext context)
    {
        if (context.instruction == null)
            return null;

        return context.instruction.Previous;
    }

    public bool TryMatch(MatchContext context)
    {
        var instruction = context.instruction;
        Match(context);

        if (context.success)
            return true;

        context.Reset(instruction);
        return false;
    }

    public static MatchContext Match(MethodBase method, ILPattern pattern)
    {
        if (method == null)
            throw new ArgumentNullException("method");
        if (pattern == null)
            throw new ArgumentNullException("pattern");

        var instructions = method.GetInstructions();
        if (instructions.Count == 0)
            throw new ArgumentException();

        var context = new MatchContext(instructions[0]);
        pattern.Match(context);
        return context;
    }
}

public sealed class MatchContext
{

    internal Instruction instruction;
    internal bool success;

    Dictionary<object, object> data = new Dictionary<object, object>();

    public bool IsMatch
    {
        get { return success; }
        set { success = true; }
    }

    internal MatchContext(Instruction instruction)
    {
        Reset(instruction);
    }

    public bool TryGetData(object key, out object value)
    {
        return data.TryGetValue(key, out value);
    }

    public void AddData(object key, object value)
    {
        data.Add(key, value);
    }

    internal void Reset(Instruction instruction)
    {
        this.instruction = instruction;
        this.success = true;
    }

    internal void Advance()
    {
        this.instruction = this.instruction.Next;
    }
}

public sealed class Instruction
{

    int offset;
    OpCode opcode;
    object operand;

    Instruction previous;
    Instruction next;

    public int Offset
    {
        get { return offset; }
    }

    public OpCode OpCode
    {
        get { return opcode; }
    }

    public object Operand
    {
        get { return operand; }
        internal set { operand = value; }
    }

    public Instruction Previous
    {
        get { return previous; }
        internal set { previous = value; }
    }

    public Instruction Next
    {
        get { return next; }
        internal set { next = value; }
    }

    public int Size
    {
        get
        {
            int size = opcode.Size;

            switch (opcode.OperandType)
            {
                case OperandType.InlineSwitch:
                    size += (1 + ((Instruction[ ])operand).Length) * 4;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    size += 8;
                    break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    size += 4;
                    break;
                case OperandType.InlineVar:
                    size += 2;
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    size += 1;
                    break;
            }

            return size;
        }
    }

    internal Instruction(int offset, OpCode opcode)
    {
        this.offset = offset;
        this.opcode = opcode;
    }

    public override string ToString()
    {
        var instruction = new StringBuilder();

        AppendLabel(instruction, this);
        instruction.Append(':');
        instruction.Append(' ');
        instruction.Append(opcode.Name);

        if (operand == null)
            return instruction.ToString();

        instruction.Append(' ');

        switch (opcode.OperandType)
        {
            case OperandType.ShortInlineBrTarget:
            case OperandType.InlineBrTarget:
                AppendLabel(instruction, (Instruction)operand);
                break;
            case OperandType.InlineSwitch:
                var labels = (Instruction[ ])operand;
                for (int i = 0; i < labels.Length; i++)
                {
                    if (i > 0)
                        instruction.Append(',');

                    AppendLabel(instruction, labels[i]);
                }
                break;
            case OperandType.InlineString:
                instruction.Append('\"');
                instruction.Append(operand);
                instruction.Append('\"');
                break;
            default:
                instruction.Append(operand);
                break;
        }

        return instruction.ToString();
    }

    static void AppendLabel(StringBuilder builder, Instruction instruction)
    {
        builder.Append("IL_");
        builder.Append(instruction.offset.ToString("x4"));
    }
}

class MethodBodyReader
{

    static readonly OpCode[ ] one_byte_opcodes;
    static readonly OpCode[ ] two_bytes_opcodes;

    static MethodBodyReader()
    {
        one_byte_opcodes = new OpCode[0xe1];
        two_bytes_opcodes = new OpCode[0x1f];

        var fields = typeof(OpCodes).GetFields(
            BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            var opcode = (OpCode)field.GetValue(null);
            if (opcode.OpCodeType == OpCodeType.Nternal)
                continue;

            if (opcode.Size == 1)
                one_byte_opcodes[opcode.Value] = opcode;
            else
                two_bytes_opcodes[opcode.Value & 0xff] = opcode;
        }
    }

    readonly MethodBase method;
    readonly MethodBody body;
    readonly Module module;
    readonly Type[ ] type_arguments;
    readonly Type[ ] method_arguments;
    readonly ByteBuffer il;
    readonly ParameterInfo this_parameter;
    readonly ParameterInfo[ ] parameters;
    readonly IList<LocalVariableInfo> locals;
    readonly List<Instruction> instructions;

    MethodBodyReader(MethodBase method)
    {
        this.method = method;

        this.body = method.GetMethodBody();
        if (this.body == null)
            throw new ArgumentException("Method has no body");

        var bytes = body.GetILAsByteArray();
        if (bytes == null)
            throw new ArgumentException("Can not get the body of the method");

        if (!(method is ConstructorInfo))
            method_arguments = method.GetGenericArguments();

        if (method.DeclaringType != null)
            type_arguments = method.DeclaringType.GetGenericArguments();

        if (!method.IsStatic)
            this.this_parameter = new ThisParameter(method);
        this.parameters = method.GetParameters();
        this.locals = body.LocalVariables;
        this.module = method.Module;
        this.il = new ByteBuffer(bytes);
        this.instructions = new List<Instruction>((bytes.Length + 1) / 2);
    }

    void ReadInstructions()
    {
        Instruction previous = null;

        while (il.position < il.buffer.Length)
        {
            var instruction = new Instruction(il.position, ReadOpCode());

            ReadOperand(instruction);

            if (previous != null)
            {
                instruction.Previous = previous;
                previous.Next = instruction;
            }

            instructions.Add(instruction);
            previous = instruction;
        }

        ResolveBranches();
    }

    void ReadOperand(Instruction instruction)
    {
        switch (instruction.OpCode.OperandType)
        {
            case OperandType.InlineNone:
                break;
            case OperandType.InlineSwitch:
                int length = il.ReadInt32();
                int base_offset = il.position + (4 * length);
                int[ ] branches = new int[length];
                for (int i = 0; i < length; i++)
                    branches[i] = il.ReadInt32() + base_offset;

                instruction.Operand = branches;
                break;
            case OperandType.ShortInlineBrTarget:
                instruction.Operand = (((sbyte)il.ReadByte()) + il.position);
                break;
            case OperandType.InlineBrTarget:
                instruction.Operand = il.ReadInt32() + il.position;
                break;
            case OperandType.ShortInlineI:
                if (instruction.OpCode == OpCodes.Ldc_I4_S)
                    instruction.Operand = (sbyte)il.ReadByte();
                else
                    instruction.Operand = il.ReadByte();
                break;
            case OperandType.InlineI:
                instruction.Operand = il.ReadInt32();
                break;
            case OperandType.ShortInlineR:
                instruction.Operand = il.ReadSingle();
                break;
            case OperandType.InlineR:
                instruction.Operand = il.ReadDouble();
                break;
            case OperandType.InlineI8:
                instruction.Operand = il.ReadInt64();
                break;
            case OperandType.InlineSig:
                instruction.Operand = module.ResolveSignature(il.ReadInt32());
                break;
            case OperandType.InlineString:
                instruction.Operand = module.ResolveString(il.ReadInt32());
                break;
            case OperandType.InlineTok:
            case OperandType.InlineType:
            case OperandType.InlineMethod:
            case OperandType.InlineField:
                instruction.Operand = module.ResolveMember(il.ReadInt32(), type_arguments, method_arguments);
                break;
            case OperandType.ShortInlineVar:
                instruction.Operand = GetVariable(instruction, il.ReadByte());
                break;
            case OperandType.InlineVar:
                instruction.Operand = GetVariable(instruction, il.ReadInt16());
                break;
            default:
                throw new NotSupportedException();
        }
    }

    void ResolveBranches()
    {
        foreach (var instruction in instructions)
        {
            switch (instruction.OpCode.OperandType)
            {
                case OperandType.ShortInlineBrTarget:
                case OperandType.InlineBrTarget:
                    instruction.Operand = GetInstruction(instructions, (int)instruction.Operand);
                    break;
                case OperandType.InlineSwitch:
                    var offsets = (int[ ])instruction.Operand;
                    var branches = new Instruction[offsets.Length];
                    for (int j = 0; j < offsets.Length; j++)
                        branches[j] = GetInstruction(instructions, offsets[j]);

                    instruction.Operand = branches;
                    break;
            }
        }
    }

    static Instruction GetInstruction(List<Instruction> instructions, int offset)
    {
        var size = instructions.Count;
        if (offset < 0 || offset > instructions[size - 1].Offset)
            return null;

        int min = 0;
        int max = size - 1;
        while (min <= max)
        {
            int mid = min + ((max - min) / 2);
            var instruction = instructions[mid];
            var instruction_offset = instruction.Offset;

            if (offset == instruction_offset)
                return instruction;

            if (offset < instruction_offset)
                max = mid - 1;
            else
                min = mid + 1;
        }

        return null;
    }

    object GetVariable(Instruction instruction, int index)
    {
        return TargetsLocalVariable(instruction.OpCode)
            ? (object)GetLocalVariable(index)
            : (object)GetParameter(index);
    }

    static bool TargetsLocalVariable(OpCode opcode)
    {
        return opcode.Name.Contains("loc");
    }

    LocalVariableInfo GetLocalVariable(int index)
    {
        return locals[index];
    }

    ParameterInfo GetParameter(int index)
    {
        if (method.IsStatic)
            return parameters[index];

        if (index == 0)
            return this_parameter;

        return parameters[index - 1];
    }

    OpCode ReadOpCode()
    {
        byte op = il.ReadByte();
        return op != 0xfe
            ? one_byte_opcodes[op]
            : two_bytes_opcodes[il.ReadByte()];
    }

    public static List<Instruction> GetInstructions(MethodBase method)
    {
        var reader = new MethodBodyReader(method);
        reader.ReadInstructions();
        return reader.instructions;
    }

    class ThisParameter : ParameterInfo
    {
        public ThisParameter(MethodBase method)
        {
            this.MemberImpl = method;
            this.ClassImpl = method.DeclaringType;
            this.NameImpl = "this";
            this.PositionImpl = -1;
        }
    }
}

public static class Disassembler
{

    public static IList<Instruction> GetInstructions(this MethodBase self)
    {
        if (self == null)
            throw new ArgumentNullException("self");

        return MethodBodyReader.GetInstructions(self).AsReadOnly();
    }
}


class ByteBuffer
{

    internal byte[ ] buffer;
    internal int position;

    public ByteBuffer(byte[ ] buffer)
    {
        this.buffer = buffer;
    }

    public byte ReadByte()
    {
        CheckCanRead(1);
        return buffer[position++];
    }

    public byte[ ] ReadBytes(int length)
    {
        CheckCanRead(length);
        var value = new byte[length];
        Buffer.BlockCopy(buffer, position, value, 0, length);
        position += length;
        return value;
    }

    public short ReadInt16()
    {
        CheckCanRead(2);
        short value = (short)(buffer[position]
            | (buffer[position + 1] << 8));
        position += 2;
        return value;
    }

    public int ReadInt32()
    {
        CheckCanRead(4);
        int value = buffer[position]
            | (buffer[position + 1] << 8)
            | (buffer[position + 2] << 16)
            | (buffer[position + 3] << 24);
        position += 4;
        return value;
    }

    public long ReadInt64()
    {
        CheckCanRead(8);
        uint low = (uint)(buffer[position]
            | (buffer[position + 1] << 8)
            | (buffer[position + 2] << 16)
            | (buffer[position + 3] << 24));

        uint high = (uint)(buffer[position + 4]
            | (buffer[position + 5] << 8)
            | (buffer[position + 6] << 16)
            | (buffer[position + 7] << 24));

        long value = (((long)high) << 32) | low;
        position += 8;
        return value;
    }

    public float ReadSingle()
    {
        if (!BitConverter.IsLittleEndian)
        {
            var bytes = ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        CheckCanRead(4);
        float value = BitConverter.ToSingle(buffer, position);
        position += 4;
        return value;
    }

    public double ReadDouble()
    {
        if (!BitConverter.IsLittleEndian)
        {
            var bytes = ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        CheckCanRead(8);
        double value = BitConverter.ToDouble(buffer, position);
        position += 8;
        return value;
    }

    void CheckCanRead(int count)
    {
        if (position + count > buffer.Length)
            throw new ArgumentOutOfRangeException();
    }
}
