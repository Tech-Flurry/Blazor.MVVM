//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TechFlurry.Blazor.MVVM.Abstractions;
//public abstract class MethodInvoker<TAttribute> where TAttribute : class
//{
//    private readonly Type _attributeType;

//    internal MethodInvoker()
//    {
//        _attributeType = typeof(TAttribute);
//    }

//    public void Execute(Action action)
//    {
//        var attr = action.Method.GetCustomAttributes(_attributeType, true).First() as TAttribute;
//        var method1 = action.Target.GetType().GetMethod(attr.PreAction);
//        var method2 = action.Target.GetType().GetMethod(attr.PostAction);

//        // now first invoke the pre-action method
//        method1.Invoke(null, null);
//        // the actual action
//        action();
//        // the post-action
//        method2.Invoke(null, null);
//    }
//}
