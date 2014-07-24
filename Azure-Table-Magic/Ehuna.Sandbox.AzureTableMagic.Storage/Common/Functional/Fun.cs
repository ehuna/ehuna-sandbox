using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Ehuna.Sandbox.AzureTableMagic.Storage.Common.Functional
{
    /// <summary>
    /// Helper methods of a functional nature
    /// </summary>
    public
    class Fun
    {
        /// <summary>
        /// Passes object to the action. Returns object
        /// Use as 
        ///		return Apply(Add, obj);
        /// Instead of 
        ///		Object obj = new Object();
        ///		Add(obj);
        ///		return obj;
        /// </summary>
        public
        static
        T
        Apply<T>(
            Action<T> work,
            T obj)
        {
            work(obj);

            return obj;
        }

    }

    /// <summary>
    /// Simple wrappers to turn actions to functions and other glue code used create expressions for chaining.
    /// 
    /// Pipe
    /// PipeIf
    /// 
    /// Do
    /// DoIf
    /// 
    /// PipeNull
    /// PipeNotNull
    /// DoNull
    /// DoNotNull
    /// 
    /// IfAct
    /// IfDo
    /// 
    /// </summary>
    [DebuggerNonUserCode]
    static
    public
    class FunUtil
    {

        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ForEach equivalent for IEnumerable
        /// </summary>
        public static
        bool
        Pull<T>(
            this IEnumerable<T> source)
        {
            foreach (T item in source)
                ;

            return true;
        }

        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------
        // Apply passes an object, performs actions, and return the original object

        /// <summary>
        /// Passes an object, performs actions, and return the original object
        /// </summary>
        public
        static
        T
        Pipe<T>(
            this T obj,
            Action<T> work)
        {
            work(obj);

            return obj;
        }

        /// <summary>
        /// Passes an object, performs actions, based on a boolean, and return the original object
        /// </summary>
        public
        static
        T
        PipeIf<T>(
            this T obj,
            bool isTrue,
            Action<T> work)
        {
            if (isTrue)
                work(obj);

            return obj;
        }

        /// <summary>
        /// Passes an object, performs actions, based on a predicate, and return the original object
        /// </summary>
        public
        static
        T
        PipeIf<T>(
            this T obj,
            Func<T, bool> isTrue,
            Action<T> work)
        {
            if (isTrue(obj))
                work(obj);

            return obj;
        }

        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------
        // Apply passes an object, performs a function, and return the result of the function

        /// <summary>
        /// passes an object, performs a function, and return the result of the function
        /// </summary>
        public
        static
        TOut
        Do<TIn, TOut>(
            this TIn obj,
            Func<TIn, TOut> work)
        {
            return work(obj);
        }

        /// <summary>
        /// passes an object, performs a function based on a boolean, and return the result of the function
        /// </summary>
        public
        static
        TOut
        DoIf<TIn, TOut>(
            this TIn obj,
            bool isTrue,
            Func<TIn, TOut> onTrue)
        {
            return isTrue
                        ? onTrue(obj)
                        : default(TOut);
        }

        /// <summary>
        /// passes an object, performs a function based on a predicate, and return the result of the function
        /// </summary>
        public
        static
        TOut
        DoIf<TIn, TOut>(
            this TIn obj,
            Func<TIn, bool> isTrue,
            Func<TIn, TOut> onTrue)
        {
            return isTrue(obj)
                        ? onTrue(obj)
                        : default(TOut);
        }

        /// <summary>
        /// passes an object, performs a function based on a predicate, and return the result of the onTrue or onFalse function,
        /// as the case may be. 
        /// </summary>
        public
        static
        TOut
        DoIf<TIn, TOut>(
            this TIn obj,
            Func<TIn, bool> isTrue,
            Func<TIn, TOut> onTrue,
            Func<TIn, TOut> onFalse)
        {
            return isTrue(obj)
                        ? onTrue(obj)
                        : onFalse(obj);
        }

        /// <summary>
        /// passes an object, performs a function based on a predicate, and return the result of the onTrue function or the false value,
        /// as the case may be. 
        /// </summary>
        public
        static
        TOut
        DoIf<TIn, TOut>(
            this TIn obj,
            Func<TIn, bool> isTrue,
            Func<TIn, TOut> onTrue,
            TOut falseValue)
        {
            return isTrue(obj)
                        ? onTrue(obj)
                        : falseValue;
        }

        /// <summary>
        /// passes an object, performs a function based on a bool, and return the result of the onTrue or onFalse function,
        /// as the case may be. 
        /// </summary>
        public
        static
        TOut
        DoIf<TIn, TOut>(
            this TIn obj,
            bool isTrue,
            Func<TIn, TOut> onTrue,
            Func<TIn, TOut> onFalse)
        {
            return isTrue
                        ? onTrue(obj)
                        : onFalse(obj);
        }

        /// <summary>
        /// passes an object, performs a function based on a bool, and return the result of the onTrue function or falseValue,
        /// as the case may be. 
        /// </summary>
        public
        static
        TOut
        DoIf<TIn, TOut>(
            this TIn obj,
            bool isTrue,
            Func<TIn, TOut> onTrue,
            TOut falseValue)
        {
            return isTrue
                        ? onTrue(obj)
                        : falseValue;
        }

        /// <summary>
        /// passes an object, performs a function based on a bool, and return the result of the onTrue function or falseValue,
        /// as the case may be. 
        /// </summary>
        public
        static
        TOut
        DoIf<TIn, TOut>(
            this TIn obj,
            bool isTrue,
            Func<TIn, TOut> onTrue,
            Func<TOut> onFalse)
        {
            return isTrue
                        ? onTrue(obj)
                        : onFalse();
        }

        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------
        // IEnumerable / IQuerable wrappers

        /// <summary>
        /// Applies a where clause, if true else returns the enumerable
        /// </summary>
        public
        static
        IEnumerable<T>
        IfWhere<T>(
            this IEnumerable<T> enumerable,
            bool isTrue,
            Func<T, bool> predicate)
        {
            return isTrue
                        ? enumerable.Where(predicate)
                        : enumerable;
        }

        /// <summary>
        /// Applies a where clause, if true else returns the queryable
        /// </summary>
        public
        static
        IQueryable<T>
        IfWhere<T>(
            this IQueryable<T> enumerable,
            bool isTrue,
            Expression<Func<T, bool>> predicate)
        {
            return isTrue
                        ? enumerable.Where(predicate)
                        : enumerable;
        }

        /// <summary>
        /// Applies a where clause, if not null else returns the queryable
        /// </summary>
        public
        static
        IQueryable<T>
        WhereNotNull<T>(
            this IQueryable<T> enumerable,
            Expression<Func<T, bool>> predicate)
        {
            return
                enumerable
                .IfWhere(
                        predicate != null,
                        predicate);
        }

        // -------------------------------------------------------------------------------------------------------------
        // Not Null 

        /// <summary>
        /// On not null applies the function and returns result. Else returns null.
        /// </summary>
        public
        static
        TOut
        DoNotNull<TIn, TOut>(
            this TIn obj,
            Func<TIn, TOut> onNotNull)
        where
            TIn : class
        {
            return obj != null
                        ? onNotNull(obj)
                        : default(TOut);
        }

        /// <summary>
        /// On not null applies the function and returns result. Else returns value returned by onElse.
        /// </summary>
        public
        static
        TOut
        DoNotNull<TIn, TOut>(
            this TIn obj,
            Func<TIn, TOut> onNotNull,
            Func<TOut> onElse)
        where
            TIn : class
        {
            return obj != null
                        ? onNotNull(obj)
                        : onElse();
        }

        /// <summary>
        /// On not null applies the function and returns result. Else returns defaultValue.
        /// </summary>
        public
        static
        TOut
        DoNotNull<TIn, TOut>(
            this TIn obj,
            Func<TIn, TOut> onNotNull,
            TOut defaultValue)
        where
            TIn : class
        {
            return obj != null
                        ? onNotNull(obj)
                        : defaultValue;
        }

        /// <summary>
        /// If object is not null applies the action. Return the object.
        /// </summary>
        public
        static
        T
        PipeNotNull<T>(
            this T obj,
            Action<T> onNotNull)
        where
            T : class
        {
            if (obj != null)
                onNotNull(obj);

            return obj;
        }

        // -------------------------------------------------------------------------------------------------------------
        // Null

        /// <summary>
        /// If object is null applies the function and returns result. Else returns the object. (a ?? y)
        /// </summary>
        public
        static
        T
        DoNull<T>(
            this T obj,
            Func<T> onNull)
        where
            T : class
        {
            return obj ?? onNull();
        }

        public
        static
        T
        PipeNull<T>(
            this T obj,
            Action onNull)
        where
            T : class
        {
            if (obj == null)
                onNull();

            return obj;
        }

        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------
        // Not Default

        public
        static
        bool
        IsDefault<T>(
            this T obj)
        {
            return EqualityComparer<T>.Default.Equals(obj, default(T));
        }

        /// <summary>
        /// If object is not default applies the function and returns result. Else returns default.
        /// </summary>
        public
        static
        TOut
        DoNotDefault<TIn, TOut>(
            this TIn obj,
            Func<TIn, TOut> onNotDefault)
        {
            return !obj.IsDefault()
                        ? onNotDefault(obj)
                        : default(TOut);
        }

        /// <summary>
        /// Returns result this/else functions depending on 'this' passed in, 
        /// </summary>
        public
        static
        TOut
        DoNotDefault<TIn, TOut>(
            this TIn obj,
            Func<TIn, TOut> onNotDefault,
            Func<TOut> onElse)
        {
            return !obj.IsDefault()
                        ? onNotDefault(obj)
                        : onElse();
        }

        // -------------------------------------------------------------------------------------------------------------
        // Default

        /// <summary>
        /// If object is default applies the function and returns result. Else returns the object. (a ?? y)
        /// </summary>
        public
        static
        T
        DoDefault<T>(
            this T obj,
            Func<T> onDefault)
        {
            return obj.IsDefault()
                        ? onDefault()
                        : obj;
        }

        public
        static
        T
        PipeDefault<T>(
            this T obj,
            Action onDefault)
        {
            if (obj.IsDefault())
                onDefault();

            return obj;
        }


        // -------------------------------------------------------------------------------------------------------------
        // Boolean

        /// <summary>
        /// If true, call action, return boolean
        /// </summary>
        public
        static
        bool
        IfPipe(
            this bool isTrue,
            Action onTrue)
        {
            if (isTrue)
                onTrue();

            return isTrue;
        }

        /// <summary>
        /// If NOT true, call action, returns the boolean
        /// </summary>
        public
        static
        bool
        IfNotPipe(
            this bool isTrue,
            Action onFalse)
        {
            if (!isTrue)
                onFalse();

            return isTrue;
        }

        /// <summary>
        /// calls this/else actions depending on 'this' passed in, returns boolean 
        /// </summary>
        public
        static
        bool
        IfPipe(
            this bool isTrue,
            Action onTrue,
            Action onElse)
        {
            if (isTrue)
                onTrue();
            else
                onElse();

            return isTrue;
        }

        /// <summary>
        /// Returns result this/else functions depending on 'this' passed in, 
        /// </summary>
        public
        static
        T
        IfDo<T>(
            this bool isTrue,
            Func<T> onTrue,
            Func<T> onElse)
        {
            return isTrue
                        ? onTrue()
                        : onElse();
        }

        public
        static
        T
        IfDo<T>(
            this bool isTrue,
            Func<T> onTrue,
            T elseValue)
        {
            return isTrue
                        ? onTrue()
                        : elseValue;
        }

        public
        static
        T
        IfDo<T>(
            this bool isTrue,
            Func<T> onTrue)
        {
            return isTrue
                        ? onTrue()
                        : default(T);
        }

        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------

        public
        static
        TOut
        DoUsing<TIn, TOut>(
            this TIn obj,
            Func<TIn, TOut> work)
        where
            TIn : IDisposable
        {
            TOut result;

            using (obj)
                result = work(obj);

            return result;
        }

        // -------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------

        public
        static
        TOut
        Return<TIn, TOut>(
            this TIn inIgnore,
            TOut obj)
        {
            return obj;
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    public
    delegate
    void
    Formatter(
        string format,
        params object[] p);

    public
    static
    class FunUtilExperimental
    {
        public
        static
        Bound1<TParam1>
        Bind<T, TParam1>(
            this T obj,
            Func<T, Action<TParam1>> work)
        {
            return
                new Bound1<TParam1>(
                                work(obj));
        }

        public
        static
        BoundFormatter<T>
        BindFormatter<T>(
            this T obj,
            Func<
                T,
                Formatter> work)
        {
            return
                new BoundFormatter<T>(
                                obj,
                                work(obj));
        }

        public
        static
        void
        Usage()
        {
            var xxx1 =
                new BindTest()
                .Bind<BindTest, int>(
                    x => y => x.Act(y))
                .Do(1)
                .Do(1);

            //Bind(
            //    new BindTest(),
            //    (BindTest x) => (int y) => x.Act(y) )
            //    .Do(1);
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    public
    class BindTest
    {
        int _count;

        public
        void
        Act(
            int i)
        {
            _count += i;
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    public
    class Bound1<TParam1>
    {
        readonly
        Action<TParam1> _work;

        public
        Bound1(
            Action<TParam1> work)
        {
            _work = work;
        }

        public
        Bound1<TParam1>
        Do(
            TParam1 param)
        {
            _work(param);

            return this;
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    public
    class Bound2<TParam1, TParam2>
    {
        readonly
        Action<TParam1, TParam2> _work;

        public
        Bound2(
            Action<TParam1, TParam2> work)
        {
            _work = work;
        }

        public
        Bound2<TParam1, TParam2>
        Do(
            TParam1 param1,
            TParam2 param2)
        {
            _work(param1, param2);

            return this;
        }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    public
    class BoundFormatter<T>
    {
        readonly
        T _obj;

        readonly
        Formatter _work;

        public
        BoundFormatter(
            T obj,
            Formatter work)
        {
            _obj = obj;
            _work = work;
        }

        public
        BoundFormatter<T>
        Do(
            string format,
            params object[] p)
        {
            _work(format, p);

            return this;
        }

        public
        T
        Unbind()
        {
            return _obj;
        }

    }

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
}
