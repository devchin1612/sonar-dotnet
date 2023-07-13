﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;

var constantIV = new byte[16];

using var aes = new AesCng();
using var rng = new RNGCryptoServiceProvider();

aes.CreateEncryptor();
aes.CreateEncryptor(aes.Key, new byte[16]); // FIXME Non-compliant

void TopLevelLocalFunction()
{
    using var aes = new AesCng();
    aes.CreateEncryptor();
    aes.CreateEncryptor(aes.Key, new byte[16]); // FIXME Non-compliant
}

public class Sample
{
    public void TargetTypedNew()
    {
        AesCng aes = new();
        aes.CreateEncryptor();
        aes.CreateEncryptor(aes.Key, new byte[16]); // FIXME Non-compliant
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            AesCng aes = new AesCng();
            aes.CreateEncryptor();
            aes.CreateEncryptor(aes.Key, new byte[16]); // FIXME Non-compliant
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            AesCng aes = new AesCng();
            aes.CreateEncryptor(); // FIXME Non-compliant
            aes.GenerateIV();
            aes.CreateEncryptor();
        }
    }

    // for code coverage
    public void CollectionInitializer()
    {
        Dictionary<int, string> students = new()
        {
            { 111, "a" },
        };
    }
}

public record Record
{
    public void Method()
    {
        AesCng aes = new AesCng();
        aes.CreateEncryptor();
        aes.CreateEncryptor(aes.Key, new byte[16]); // FIXME Non-compliant
    }
}

public partial class Partial
{
    public partial void Method();
}

public partial class Partial
{
    public partial void Method()
    {
        AesCng aes = new AesCng();
        aes.CreateEncryptor();
        aes.CreateEncryptor(aes.Key, new byte[16]); // FIXME Non-compliant
    }
}

namespace TargetTypedConditional
{
    public class Sample
    {
        public void Go(bool condition)
        {
            SymmetricAlgorithm aes = condition ? new AesCng() : new AesCryptoServiceProvider();
            aes.CreateEncryptor();
            aes.CreateEncryptor(aes.Key, new byte[16]); // FIXME Non-compliant
        }
    }
}