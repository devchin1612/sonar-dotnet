﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Web.UI;

internal class Serializer
{
    internal void BinaryFormatterDeserialize(MemoryStream memoryStream)
    {
        new BinaryFormatter().Deserialize(memoryStream);                    // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        new BinaryFormatter { TypeFormat = FormatterTypeStyle.TypesWhenNeeded };
    }

    internal void NetDataContractSerializerDeserialize()
    {
        new NetDataContractSerializer().Deserialize(new MemoryStream());    // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        new NetDataContractSerializer { }.Deserialize(new MemoryStream());  // Noncompliant
    }

    internal void SoapFormatterDeserialize()
    {
        new SoapFormatter().Deserialize(new MemoryStream());                // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        new SoapFormatter { }.Deserialize(new MemoryStream());              // Noncompliant
    }

    internal void BinderAsVariable(Stream stream, bool condition)
    {
        var safeBinder = new SafeBinderStatementWithReturnNull();
        var unsafeBinder = new UnsafeBinder();
        SerializationBinder nullBinder = null;

        var formatter1 = new BinaryFormatter();
        formatter1.Binder = safeBinder;
        formatter1.Deserialize(stream);                                     // Compliant: safe binder was used

        var formatter2 = new BinaryFormatter();
        formatter2.Binder = unsafeBinder;
        formatter2.Deserialize(stream);                                     // Noncompliant [unsafeBinder1]: unsafe binder used

        var formatter3 = new BinaryFormatter();
        formatter3.Binder = nullBinder;
        formatter3.Deserialize(stream);                                     // Noncompliant: the binder is null

        var possibleNullBinder = condition ? null : new SafeBinderStatementWithReturnNull();
        var formatter4 = new BinaryFormatter();
        formatter4.Binder = possibleNullBinder;
        formatter4.Deserialize(stream);                                     // Noncompliant: the binder can be null

        var formatter5 = new BinaryFormatter();
        if (condition)
        {
            formatter5.Binder = new SafeBinderStatementWithReturnNull();
        }
        formatter5.Deserialize(stream);                                     // Noncompliant: the binder can be null

        var formatter6 = new BinaryFormatter();
        formatter6.Binder = new SafeBinderExpressionWithNull();
        formatter6.Binder = new UnsafeBinder();
        formatter6.Deserialize(stream);                                     // Noncompliant [unsafeBinder2]: the last binder set is unsafe

        var formatter7 = new BinaryFormatter { Binder = new SafeBinderExpressionWithNull() };
        formatter7.Binder = new UnsafeBinder();
        formatter7.Deserialize(stream);                                     // Noncompliant [unsafeBinder3]: the last binder set is unsafe

        var formatter8 = new BinaryFormatter();
        formatter8.Binder = new UnsafeBinder();
        formatter8.Binder = new SafeBinderExpressionWithNull();
        formatter8.Deserialize(stream);                                     // Compliant: the last binder set is safe

        var formatter9 = new BinaryFormatter { Binder = new UnsafeBinder() };
        formatter9.Binder = new SafeBinderExpressionWithNull();
        formatter9.Deserialize(stream);                                     // Compliant: the last binder set is safe

        var formatter10 = new BinaryFormatter { Binder = new UnsafeBinder() };
        formatter10.Deserialize(stream);                                    // Noncompliant [unsafeBinder4]: the safe binder was set after deserialize call
        formatter10.Binder = new SafeBinderExpressionWithNull();

        var formatter15 = new BinaryFormatter();
        var formatter16 = new BinaryFormatter();
        try
        {
            formatter15.Binder = new SafeBinderExpressionWithNull();
            formatter15.Deserialize(stream);                                // Compliant: safe binder

            formatter16.Binder = new UnsafeBinder();
            formatter16.Deserialize(stream);                                // Noncompliant [unsafeBinder5]: unsafe binder
        }
        catch
        {
            formatter15.Deserialize(stream);                                // Noncompliant
            formatter16.Deserialize(stream);                                // Noncompliant
        }

        while (true)
        {
            var formatter17 = new BinaryFormatter { Binder = new SafeBinderExpressionWithNull() };
            formatter17.Deserialize(stream);                                // Compliant: safe binder

            var formatter18 = new BinaryFormatter { Binder = new UnsafeBinder() };
            formatter18.Deserialize(stream);                                // Noncompliant [unsafeBinder6]: unsafe binder
        }
    }

    private void Functions(Stream stream)
    {
        void LocalFunction()
        {
            new BinaryFormatter { Binder = new SafeBinderExpressionWithNull() }.Deserialize(stream);            // Compliant: safe binder

            new BinaryFormatter { Binder = new UnsafeBinder() }.Deserialize(stream);                            // Noncompliant [unsafeBinder7]: unsafe binder used
        }

        Func<UnsafeBinder> binderFactoryUnsafe = () => new UnsafeBinder();
        new BinaryFormatter { Binder = binderFactoryUnsafe() }.Deserialize(stream);                             // Noncompliant [unsafeBinder8]: unsafe binder used

        Func<SafeBinderExpressionWithNull> binderFactorySafe = () => new SafeBinderExpressionWithNull();
        new BinaryFormatter { Binder = binderFactorySafe() }.Deserialize(stream);                               // Compliant: safe binder used

        new BinaryFormatter { Binder = BinderFactoryUnsafe() }.Deserialize(stream);                             // Noncompliant [unsafeBinder9]: unsafe binder used
        new BinaryFormatter { Binder = BinderFactorySafe() }.Deserialize(stream);                               // Compliant: safe binder used
    }

    private static UnsafeBinder BinderFactoryUnsafe() => new UnsafeBinder();
    private static SafeBinderExpressionWithNull BinderFactorySafe() => new SafeBinderExpressionWithNull();

    internal void DeserializeOnExpression(MemoryStream memoryStream, bool condition)
    {
        new BinaryFormatter().Deserialize(memoryStream);                                                        // Noncompliant - Unsafe by default
        new BinaryFormatter { Binder = null }.Deserialize(memoryStream);                                        // Noncompliant - Unsafe when the binder is null
        (condition ? new BinaryFormatter() : null).Deserialize(memoryStream);                                   // Noncompliant - Unsafe in ternary operator
        BinaryFormatter bin = null;
        new BinaryFormatter { Binder = new SafeBinderStatementWithReturnNull() }.Deserialize(memoryStream);     // safe binder set in initializer
        new BinaryFormatter { Binder = new UnsafeBinder() }.Deserialize(memoryStream);                          // Noncompliant [unsafeBinder10]: unsafe binder set in initializer
        (condition
            ? new BinaryFormatter { Binder = new SafeBinderStatementWithReturnNull() }
            : new BinaryFormatter { Binder = new SafeBinderWithThrowStatement() }).Deserialize(memoryStream);   // Safe in ternary operator
    }

    internal void WithInitializeInsideIf(Stream stream)
    {
        BinaryFormatter formatter;
        if ((formatter = new BinaryFormatter()) != null)
        {
            formatter.Deserialize(stream);                                      // Noncompliant - Unsafe by default
        }
    }

    internal void Switch(Stream stream, bool condition, int number)
    {
        var formatter4 = new BinaryFormatter();
        switch (number)
        {
            case 1:
                formatter4.Deserialize(stream);                                 // Noncompliant: null binder
                break;
            case 2:
                formatter4.Binder = null;
                formatter4.Deserialize(stream);                                 // Noncompliant [nullBinder1]: null binder
                break;
            case 3:
                formatter4.Binder = new UnsafeBinder();
                formatter4.Deserialize(stream);                                 // Noncompliant [unsafeBinder11]: unsafe binder
                break;
            default:
                formatter4.Binder = new SafeBinderExpressionWithNull();
                formatter4.Deserialize(stream);                                 // Compliant: safe binder
                break;
        }
    }

    internal void BinderCases(MemoryStream memoryStream)
    {
        var formatter = new BinaryFormatter();

        formatter.Binder = new SafeBinderStatementWithReturnNull();
        formatter.Deserialize(memoryStream);                                    // Compliant: a safe binder was used

        formatter.Binder = new SafeBinderExpressionWithNull();
        formatter.Deserialize(memoryStream);                                    // Compliant: a safe binder was used

        formatter.Binder = new SafeBinderWithThrowStatement();
        formatter.Deserialize(memoryStream);                                    // Compliant: a safe binder was used

        formatter.Binder = new SafeBinderWithThrowExpression();
        formatter.Deserialize(memoryStream);                                    // Compliant: a safe binder was used

        formatter.Binder = new UnsafeBinder();
        formatter.Deserialize(memoryStream);                                    // Noncompliant [unsafeBinder12]: the used binder does not validate the deserialized types

        formatter.Binder = new UnsafeBinderExpressionBody();
        formatter.Deserialize(memoryStream);                                    // Noncompliant: the used binder does not validate the deserialized types

        formatter.Binder = new SafeBinderWithOtherMethods();
        formatter.Deserialize(memoryStream);                                    // Compliant: safe binder

        formatter.Binder = new UnsafeBinderWithOtherMethods();
        formatter.Deserialize(memoryStream);                                    // Noncompliant: unsafe binder
    }

    internal void UnknownBindersAreSafe(SerializationBinder binder, bool condition)
    {
        new BinaryFormatter { Binder = binder }.Deserialize(new MemoryStream());// Compliant: Unknown binders are considered safe

        var formatter = new BinaryFormatter { Binder = binder };
        formatter.Deserialize(new MemoryStream());                              // Compliant: Unknown binders are considered safe

        new BinaryFormatter()                                                   // Noncompliant [unsafeBinder13]: unsafe binder on at least one path
        {
            Binder = condition ? (SerializationBinder) new SafeBinderExpressionWithNull() : new UnsafeBinder()
        }.Deserialize(new MemoryStream());                                      
    }

    internal BinaryFormatter UnknownBinaryFormattersAreSafeByDefault(BinaryFormatter formatter)
    {
        formatter.Deserialize(new MemoryStream());                              // Compliant
        formatter.Binder = new UnsafeBinder();
        formatter.Deserialize(new MemoryStream());                              // Noncompliant [unsafeBinder14]

        formatter = UnknownBinaryFormattersAreSafeByDefault(null);
        formatter.Deserialize(new MemoryStream());                              // Compliant
        formatter.Binder = new UnsafeBinder();
        formatter.Deserialize(new MemoryStream());                              // Noncompliant [unsafeBinder15]

        BinaryFormatterField.Deserialize(new MemoryStream());                   // Compliant
        BinaryFormatterField.Binder = new UnsafeBinder();
        BinaryFormatterField.Deserialize(new MemoryStream());                   // Noncompliant [unsafeBinder16]

        return null;
    }

    internal void UnknownFormattersAreSafeByDefault(IFormatter formatter)
    {
        formatter.Binder = new UnsafeBinder();
        formatter.Deserialize(new MemoryStream());                              // Compliant
    }

    BinaryFormatter BinaryFormatterField;
}

namespace Aliases
{
    using AliasedBinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

    class BinaryFormatter
    {
        SerializationBinder Binder { get; set; }
        object Deserialize(Stream serializationStream) => null;

        void Test()
        {
            new AliasedBinaryFormatter().Deserialize(new MemoryStream());       // Noncompliant
            new BinaryFormatter().Deserialize(new MemoryStream());              // Compliant
        }
    }

}

internal sealed class SafeBinderStatementWithReturnNull : SerializationBinder
{
    public override Type BindToType(String assemblyName, System.String typeName)
    {
        if (typeName == "typeT")
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }

        return null;
    }
}

internal sealed class SafeBinderExpressionWithNull : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName) =>
        typeName == "typeT"
            ? Assembly.Load(assemblyName).GetType(typeName)
            : null;
}

internal sealed class SafeBinderWithThrowStatement : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (typeName == "typeT")
            return Assembly.Load(assemblyName).GetType(typeName);

        throw new SerializationException("Only typeT is allowed");
    }
}

internal sealed class SafeBinderWithThrowExpression : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName) =>
        typeName == "typeT"
            ? Assembly.Load(assemblyName).GetType(typeName)
            : throw new SerializationException("Only typeT is allowed");
}

internal sealed class UnsafeBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)   // nullbinder1: FP
    //                   ^^^^^^^^^^ Secondary [unsafeBinder1, unsafeBinder2, unsafeBinder3, unsafeBinder4, unsafeBinder5, unsafeBinder6, unsafeBinder7, unsafeBinder8, unsafeBinder9, unsafeBinder10, unsafeBinder11, unsafeBinder12, unsafeBinder13, unsafeBinder14, unsafeBinder15, unsafeBinder16, nullBinder1]
    {
        return Assembly.Load(assemblyName).GetType(typeName);
    }
}

internal sealed class UnsafeBinderExpressionBody : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName) =>
//                       ^^^^^^^^^^ Secondary
        Assembly.Load(assemblyName).GetType(typeName);
}

internal sealed class UnsafeBinderWithOtherMethods : SerializationBinder
{
    public void Accept()
    {
    }

    public Type ResolveType(string id) =>
        throw new SerializationException("Not implemented.");

    public Type BindToType(string assemblyName) =>
        throw new SerializationException("Not implemented.");

    public Type BindToType(string assemblyName, int typeName) =>
        throw new SerializationException("Not implemented.");

    public override Type BindToType(string assemblyName, string typeName) =>
//                       ^^^^^^^^^^ Secondary
        Assembly.Load(assemblyName).GetType(typeName);
}

internal sealed class SafeBinderWithOtherMethods : SerializationBinder
{
    public void Accept() { }

    public Type ResolveType(string id) => Type.GetType(id);

    public override Type BindToType(string assemblyName, string typeName) =>
        throw new SerializationException("Only typeT is allowed");
}
