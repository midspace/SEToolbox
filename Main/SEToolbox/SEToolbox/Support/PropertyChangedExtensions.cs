namespace SEToolbox.Support
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    // Used from the following:
    // http://blog.decarufel.net/2009/07/how-to-use-inotifypropertychanged-type.html
    // http://blog.decarufel.net/2009/07/type-safe-inotifypropertychanged-and.html

    public static class PropertyChangedExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, params Expression<Func<object>>[] propertyExpressions)
        {
            foreach (var expression in propertyExpressions)
                Raise(handler, expression);
        }

        public static void Raise(this PropertyChangedEventHandler handler, Expression<Func<object>> propertyExpression)
        {
            if (handler != null)
            {
                // Retreive lambda body
                var body = propertyExpression.Body as MemberExpression;
                if (body == null)
                {
                    // Handling for Bool properties.
                    if (propertyExpression.Body is UnaryExpression)
                    {
                        body = (propertyExpression.Body as UnaryExpression).Operand as MemberExpression;
                    }

                    if (body == null)
                    {
                        throw new ArgumentException("'propertyExpression' should be a member expression");
                    }
                }

                // Extract the right part (after "=>")
                var vmExpression = body.Expression as ConstantExpression;
                if (vmExpression == null)
                    throw new ArgumentException("'propertyExpression' body should be a constant expression");

                // Create a reference to the calling object to pass it as the sender
                LambdaExpression vmlambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = vmlambda.Compile();
                object vm = vmFunc.DynamicInvoke();

                // Extract the name of the property to raise a change on
                string propertyName = body.Member.Name;
                var e = new PropertyChangedEventArgs(propertyName);
                handler(vm, e);
            }
        }
    }
}
