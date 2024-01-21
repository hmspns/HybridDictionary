using FluentAssertions;

namespace MoreCollections.Tests;

internal sealed class HybridDictionaryTestsFixture
{
    private readonly int _threshold;
    private readonly string _letters = "abcdefghijklmnopqrstuvwxyz";

    private readonly IEqualityComparer<string> _comparer = StringComparer.Ordinal;

    internal IEqualityComparer<string> Comparer => _comparer;

    internal int Threshold => _threshold;
    
    internal HybridDictionaryTestsFixture()
    {
        _threshold = new HybridDictionary<string, int>().ToWrapper().Threshold;
    }

    internal HybridDictionary<string, int> CreateWithDictionary()
    {
        return CreateWithDictionary(_threshold + 1);
    }

    internal HybridDictionary<string, int> CreateWithDictionary(int size, IEqualityComparer<string> comparer = null)
    {
        if (size < _threshold || size > _letters.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(size), $"Size should be between {_threshold} and {_letters.Length}");
        }

        ReadOnlySpan<char> chars = _letters.AsSpan();
        HybridDictionary<string, int> dictionary = new HybridDictionary<string, int>(size, comparer);
        for (int i = 0; i < size; i++)
        {
            dictionary.Add(new string(chars.Slice(i, 1)), i);
        }

        return dictionary;
    }

    internal HybridDictionary<string, int> Create() => new HybridDictionary<string, int>();

    internal HybridDictionary<string, int> Create(int capacity) => new HybridDictionary<string, int>(capacity);

    internal HybridDictionary<string, int> Create(IEqualityComparer<string> comparer) => new HybridDictionary<string, int>(comparer);
    
    internal HybridDictionary<string, int> Create(int capacity, IEqualityComparer<string> comparer) => new HybridDictionary<string, int>(capacity, comparer);
}

public sealed class HybridDictionaryTests
{
    private readonly HybridDictionaryTestsFixture _fixture = new HybridDictionaryTestsFixture();

    [Fact]
    public void Constructors()
    {
        HybridDictionary<string, int> dictionary = new HybridDictionary<string, int>();
        HybridDictionaryWrapper<string, int> wrapper = dictionary.ToWrapper();

        dictionary.Count.Should().Be(0);
        wrapper.Comparer.Should().BeNull();
        wrapper.List.Should().NotBeNull();
        wrapper.Dictionary.Should().BeNull();

        dictionary = new HybridDictionary<string, int>(_fixture.Comparer);
        wrapper = dictionary.ToWrapper();
        
        dictionary.Count.Should().Be(0);
        wrapper.Comparer.Should().BeSameAs(_fixture.Comparer);
        wrapper.List.Should().NotBeNull();
        wrapper.Dictionary.Should().BeNull();

        dictionary = new HybridDictionary<string, int>(_fixture.Threshold - 1);
        wrapper = dictionary.ToWrapper();
        
        dictionary.Count.Should().Be(0);
        wrapper.Comparer.Should().BeNull();
        wrapper.List.Should().NotBeNull();
        wrapper.Dictionary.Should().BeNull();
        
        dictionary = new HybridDictionary<string, int>(_fixture.Threshold + 1);
        wrapper = dictionary.ToWrapper();
        
        dictionary.Count.Should().Be(0);
        wrapper.Comparer.Should().BeNull();
        wrapper.List.Should().BeNull();
        wrapper.Dictionary.Should().NotBeNull();
        
        dictionary = new HybridDictionary<string, int>(_fixture.Threshold - 1, _fixture.Comparer);
        wrapper = dictionary.ToWrapper();
        
        dictionary.Count.Should().Be(0);
        wrapper.Comparer.Should().BeSameAs(_fixture.Comparer);
        wrapper.List.Should().NotBeNull();
        wrapper.Dictionary.Should().BeNull();
        
        dictionary = new HybridDictionary<string, int>(_fixture.Threshold + 1, _fixture.Comparer);
        wrapper = dictionary.ToWrapper();
        
        dictionary.Count.Should().Be(0);
        wrapper.Comparer.Should().BeSameAs(_fixture.Comparer);
        wrapper.List.Should().BeNull();
        wrapper.Dictionary.Should().NotBeNull();
    }
    
    [Fact]
    public void Add()
    {
        HybridDictionary<string, int> dictionary = new HybridDictionary<string, int>();

        dictionary.Count.Should().Be(0);
        
        dictionary.Add("a", 1);

        dictionary.Count.Should().Be(1);

        dictionary.AssertSwitchedToList();

        dictionary.Invoking(x => x.Add("a", 2)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddSwitch()
    {
        var dictionary = _fixture.Create();
        for (int i = 0; i < _fixture.Threshold; i++)
        {
            dictionary.AddItem();
        }
        
        dictionary.AssertSwitchedToList(_fixture.Threshold);
        
        dictionary.AddItem();
        
        dictionary.AssertSwitchedToDictionary(_fixture.Threshold + 1);
    }

    [Fact]
    public void MissedValueByIndex()
    {
        var listDictionary = _fixture.Create();
        listDictionary.AssertSwitchedToList();

        listDictionary.Invoking(x => x["some"]).Should().Throw<KeyNotFoundException>();

        var hashDictionary = _fixture.CreateWithDictionary();
        hashDictionary.AssertSwitchedToDictionary();

        hashDictionary.Invoking(x => x["some"]).Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Indexer()
    {
        var listDictionary = _fixture.Create();
        const string KEY = "some";
        const int VALUE = 42;
        const int SECOND_VALUE = 777;
        listDictionary[KEY] = VALUE;

        listDictionary.Count.Should().Be(1);
        listDictionary[KEY].Should().Be(VALUE);

        listDictionary[KEY] = SECOND_VALUE;

        listDictionary.Count.Should().Be(1);
        listDictionary[KEY].Should().Be(SECOND_VALUE);

        var hashDictionary = _fixture.CreateWithDictionary(10);
        hashDictionary[KEY] = VALUE;
        
        hashDictionary.Count.Should().Be(11);
        hashDictionary[KEY].Should().Be(VALUE);

        hashDictionary[KEY] = SECOND_VALUE;

        hashDictionary.Count.Should().Be(11);
        hashDictionary[KEY].Should().Be(SECOND_VALUE);
    }
    
    [Fact]
    public void IndexerWithComparer()
    {
        var listDictionary = _fixture.Create(StringComparer.OrdinalIgnoreCase);
        const string SET_KEY = "some";
        const string GET_KEY = "SoMe";
        const int VALUE = 42;
        const int SECOND_VALUE = 777;
        listDictionary[SET_KEY] = VALUE;

        listDictionary.Count.Should().Be(1);
        listDictionary[GET_KEY].Should().Be(VALUE);

        listDictionary[SET_KEY] = SECOND_VALUE;

        listDictionary.Count.Should().Be(1);
        listDictionary[GET_KEY].Should().Be(SECOND_VALUE);

        var hashDictionary = _fixture.CreateWithDictionary(10, StringComparer.OrdinalIgnoreCase);
        hashDictionary[SET_KEY] = VALUE;
        
        hashDictionary.Count.Should().Be(11);
        hashDictionary[GET_KEY].Should().Be(VALUE);

        hashDictionary[SET_KEY] = SECOND_VALUE;

        hashDictionary.Count.Should().Be(11);
        hashDictionary[GET_KEY].Should().Be(SECOND_VALUE);
    }
}

internal static class HybridDictionaryExtensions
{
    private static HybridDictionary<HybridDictionary<string, int>, int> _indexes = new HybridDictionary<HybridDictionary<string, int>, int>();
    
    internal static HybridDictionaryWrapper<TKey, TValue> ToWrapper<TKey, TValue>(this HybridDictionary<TKey, TValue> dictionary)
    {
        return new HybridDictionaryWrapper<TKey, TValue>(dictionary);
    }

    internal static void AssertSwitchedToList<TKey, TValue>(this HybridDictionary<TKey, TValue> dictionary, int size = Int32.MinValue)
    {
        var wrapper = dictionary.ToWrapper();
        wrapper.List.Should().NotBeNull();
        wrapper.Dictionary.Should().BeNull();

        if (size != int.MinValue)
        {
            dictionary.Count.Should().Be(size);
        }
    }
    
    internal static void AssertSwitchedToDictionary<TKey, TValue>(this HybridDictionary<TKey, TValue> dictionary, int size = Int32.MinValue)
    {
        var wrapper = dictionary.ToWrapper();
        wrapper.List.Should().BeNull();
        wrapper.Dictionary.Should().NotBeNull();

        if (size != int.MinValue)
        {
            dictionary.Count.Should().Be(size);
        }
    }

    internal static void AddItem(this HybridDictionary<string, int> dictionary)
    {
        if (!_indexes.TryGetValue(dictionary, out int index))
        {
            index = 1;
            _indexes[dictionary] = index;
        }
        
        dictionary.Add(index.ToString(), index);
        _indexes[dictionary] += 1;
    }
}

internal class HybridDictionaryWrapper<TKey, TValue>
{
    private readonly HybridDictionary<TKey, TValue> _dictionary;
    
    internal HybridDictionaryWrapper(HybridDictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
    }

    internal LinkedDictionary<TKey, TValue>? List => _dictionary.GetPrivateField<LinkedDictionary<TKey, TValue>>("_list");
    internal Dictionary<TKey, TValue>? Dictionary => _dictionary.GetPrivateField<Dictionary<TKey, TValue>>("_dictionary");
    internal IEqualityComparer<TKey>? Comparer => _dictionary.GetPrivateField<IEqualityComparer<TKey>>("_comparer");
    internal int Threshold => _dictionary.GetPrivateStaticField<int>("THRESHOLD");
}