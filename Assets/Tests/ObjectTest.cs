using System.Collections.Generic;
using NUnit.Framework;

public class ObjectTest
{
    [Test]
    public void StringHashKeyTest()
    {
        var hello1 = new Macaca.String() { Value = "Hello World" };
        var hello2 = new Macaca.String() { Value = "Hello World" };
        var diff1 = new Macaca.String() { Value = "My name is johnny" };
        var diff2 = new Macaca.String() { Value = "My name is johnny" };

        Assert.AreEqual(hello1.HashKey().Value, hello2.HashKey().Value);
        Assert.AreEqual(diff1.HashKey().Value, diff2.HashKey().Value);
        Assert.AreNotEqual(hello1.HashKey().Value, diff1.HashKey().Value);
    }

    [Test]
    public void BoolHashKeyTest()
    {
        var true1 = new Macaca.Bool() { Value = true };
        var true2 = new Macaca.Bool() { Value = true };
        var false1 = new Macaca.Bool() { Value = false };
        var false2 = new Macaca.Bool() { Value = false };

        Assert.AreEqual(true1.HashKey().Value, true2.HashKey().Value);
        Assert.AreEqual(false1.HashKey().Value, false2.HashKey().Value);
        Assert.AreNotEqual(true1.HashKey().Value, false1.HashKey().Value);
    }

    [Test]
    public void IntegerHashKeyTest()
    {
        var one1 = new Macaca.Integer() { Value = 1 };
        var one2 = new Macaca.Integer() { Value = 1 };
        var two1 = new Macaca.Integer() { Value = 2 };
        var two2 = new Macaca.Integer() { Value = 2 };

        Assert.AreEqual(one1.HashKey().Value, one2.HashKey().Value);
        Assert.AreEqual(two1.HashKey().Value, two2.HashKey().Value);
        Assert.AreNotEqual(one1.HashKey().Value, two1.HashKey().Value);
    }
}
