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

    internal HybridDictionary<string, int> CreateWithDictionary(IEqualityComparer<string> comparer = null)
    {
        return CreateWithDictionary(_threshold + 1, comparer);
    }

    internal HybridDictionary<string, int> CreateWithDictionary(int size, IEqualityComparer<string> comparer = null)
    {
        if (size < _threshold || size > _letters.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(size), $"Size should be between {_threshold} and {_letters.Length}");
        }

        return new HybridDictionary<string, int>(size, comparer);
    }

    internal HybridDictionary<string, int> Create() => new HybridDictionary<string, int>();

    internal HybridDictionary<string, int> Create(int capacity) => new HybridDictionary<string, int>(capacity);

    internal HybridDictionary<string, int> Create(IEqualityComparer<string> comparer) => new HybridDictionary<string, int>(comparer);
    
    internal HybridDictionary<string, int> Create(int capacity, IEqualityComparer<string> comparer) => new HybridDictionary<string, int>(capacity, comparer);
}

public sealed class HybridDictionaryTests
{
    private readonly HybridDictionaryTestsFixture _fixture = new HybridDictionaryTestsFixture();
    private const string MISSED_KEY = "some_key";
    private const string MISSED_KEY_ANOTHER_CASE = "SoMe_KeY";

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

        var hashDictionary = _fixture.CreateWithDictionary();
        hashDictionary[KEY] = VALUE;
        
        hashDictionary.Count.Should().Be(1);
        hashDictionary[KEY].Should().Be(VALUE);

        hashDictionary[KEY] = SECOND_VALUE;

        hashDictionary.Count.Should().Be(1);
        hashDictionary[KEY].Should().Be(SECOND_VALUE);
    }
    
    [Fact]
    public void IndexerWithComparer()
    {
        var listDictionary = _fixture.Create(StringComparer.OrdinalIgnoreCase);
        const string SET_KEY = MISSED_KEY;
        const string GET_KEY = MISSED_KEY_ANOTHER_CASE;
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
        
        hashDictionary.Count.Should().Be(1);
        hashDictionary[GET_KEY].Should().Be(VALUE);

        hashDictionary[SET_KEY] = SECOND_VALUE;

        hashDictionary.Count.Should().Be(1);
        hashDictionary[GET_KEY].Should().Be(SECOND_VALUE);
    }

    [Fact]
    public void ContainsKey()
    {
        var listDictionary = _fixture.Create();
        listDictionary.AssertSwitchedToList();
        listDictionary.ContainsKey(MISSED_KEY).Should().BeFalse();
        
        var (key, value) = listDictionary.AddItem();
        listDictionary.ContainsKey(key).Should().BeTrue();

        var hashDictionary = _fixture.CreateWithDictionary();
        hashDictionary.AssertSwitchedToDictionary();
        hashDictionary.ContainsKey(MISSED_KEY).Should().BeFalse();

        (key, value) = hashDictionary.AddItem();
        hashDictionary.ContainsKey(key).Should().BeTrue();

        listDictionary = _fixture.Create(StringComparer.OrdinalIgnoreCase);
        listDictionary.AssertSwitchedToList();
        listDictionary.ContainsKey(MISSED_KEY).Should().BeFalse();
        listDictionary.Add(MISSED_KEY, 1);
        listDictionary.ContainsKey(MISSED_KEY_ANOTHER_CASE).Should().BeTrue();

        hashDictionary = _fixture.CreateWithDictionary(StringComparer.OrdinalIgnoreCase);
        hashDictionary.AssertSwitchedToDictionary();
        hashDictionary.ContainsKey(MISSED_KEY).Should().BeFalse();
        hashDictionary.Add(MISSED_KEY, 1);
        hashDictionary.ContainsKey(MISSED_KEY_ANOTHER_CASE).Should().BeTrue();
    }
    
    [Fact]
    public void Contains()
    {
        KeyValuePair<string, int> WrongPair(KeyValuePair<string, int> p) =>
            new KeyValuePair<string, int>(p.Key, p.Value + 1);

        KeyValuePair<string, int> ignoreCasePair = new KeyValuePair<string, int>(MISSED_KEY, 1);
        
        KeyValuePair<string, int> missedPair = new KeyValuePair<string, int>(MISSED_KEY, 0);
        var listDictionary = _fixture.Create();
        listDictionary.AssertSwitchedToList();
        listDictionary.AsCollection().Contains(missedPair).Should().BeFalse();
        
        var pair = listDictionary.AddItem();
        listDictionary.AsCollection().Contains(pair).Should().BeTrue();
        listDictionary.AsCollection().Contains(WrongPair(pair)).Should().BeFalse();

        var hashDictionary = _fixture.CreateWithDictionary();
        hashDictionary.AssertSwitchedToDictionary();
        hashDictionary.AsCollection().Contains(missedPair).Should().BeFalse();

        pair = hashDictionary.AddItem();
        hashDictionary.AsCollection().Contains(pair).Should().BeTrue();
        hashDictionary.AsCollection().Contains(WrongPair(pair)).Should().BeFalse();

        listDictionary = _fixture.Create(StringComparer.OrdinalIgnoreCase);
        listDictionary.AssertSwitchedToList();
        listDictionary.AsCollection().Contains(missedPair).Should().BeFalse();
        listDictionary.Add(MISSED_KEY, 1);
        listDictionary.AsCollection().Contains(ignoreCasePair).Should().BeTrue();
        listDictionary.AsCollection().Contains(WrongPair(ignoreCasePair)).Should().BeFalse();

        hashDictionary = _fixture.CreateWithDictionary(StringComparer.OrdinalIgnoreCase);
        hashDictionary.AssertSwitchedToDictionary();
        hashDictionary.AsCollection().Contains(missedPair).Should().BeFalse();
        hashDictionary.Add(MISSED_KEY, 1);
        hashDictionary.AsCollection().Contains(ignoreCasePair).Should().BeTrue();
        hashDictionary.AsCollection().Contains(WrongPair(ignoreCasePair)).Should().BeFalse();
    }

    [Fact]
    public void Clear()
    {
        var listDictionary = _fixture.Create();
        listDictionary.AssertSwitchedToList(0);
        listDictionary.AddItem();
        listDictionary.AssertSwitchedToList(1);
        listDictionary.Clear();
        listDictionary.AssertSwitchedToList(0);

        var hashDictionary = _fixture.CreateWithDictionary();
        hashDictionary.AssertSwitchedToDictionary(hashDictionary.Count);
        hashDictionary.AddItem();
        hashDictionary.AssertSwitchedToDictionary(1);
        hashDictionary.Clear();
        hashDictionary.AssertSwitchedToDictionary(0);
    }

    [Fact]
    public void CopyTo()
    {
        KeyValuePair<string, int> defaultPair = default;
        
        var listDictionary = _fixture.Create();
        listDictionary.AssertSwitchedToList();

        var hashDictionary = _fixture.CreateWithDictionary();
        hashDictionary.AssertSwitchedToDictionary();
        
        Check(listDictionary);
        Check(hashDictionary);

        void Check(HybridDictionary<string, int> hybridDictionary)
        {
            KeyValuePair<string, int>[] buffer = new KeyValuePair<string, int>[4];
            hybridDictionary.AsCollection().CopyTo(buffer, 0);
            var p1 = hybridDictionary.AddItem();
            var p2 = hybridDictionary.AddItem();
            hybridDictionary.AsCollection().CopyTo(buffer, 0);
            buffer[0].Should().Be(p1);
            buffer[1].Should().Be(p2);
            buffer[2].Should().Be(defaultPair);
            buffer[3].Should().Be(defaultPair);
        
            Array.Clear(buffer);
        
            hybridDictionary.AsCollection().CopyTo(buffer, 1);
            buffer[0].Should().Be(defaultPair);
            buffer[1].Should().Be(p1);
            buffer[2].Should().Be(p2);
            buffer[3].Should().Be(defaultPair);
        }
    }

    [Fact]
    public void Remove()
    {
        var listDictionary = _fixture.Create();
        var hashDictionary = _fixture.CreateWithDictionary();
        
        Check(listDictionary, MISSED_KEY, MISSED_KEY,(d, count) => d.AssertSwitchedToList(count));
        Check(hashDictionary, MISSED_KEY, MISSED_KEY, (d, count) => d.AssertSwitchedToDictionary(count));

        listDictionary = _fixture.Create(StringComparer.OrdinalIgnoreCase);
        hashDictionary = _fixture.CreateWithDictionary(StringComparer.OrdinalIgnoreCase);
        
        Check(listDictionary, MISSED_KEY, MISSED_KEY_ANOTHER_CASE,(d, count) => d.AssertSwitchedToList(count));
        Check(hashDictionary, MISSED_KEY, MISSED_KEY_ANOTHER_CASE, (d, count) => d.AssertSwitchedToDictionary(count));

        void Check(HybridDictionary<string, int> dictionary, string addKey, string validateKey, Action<HybridDictionary<string, int>, int> assert)
        {
            dictionary.AddItem();
            var item = dictionary.AddItem();
            assert(dictionary, 2);

            dictionary.Remove(item.Key).Should().BeTrue();
            assert(dictionary, 1);
            
            dictionary.ContainsKey(item.Key).Should().BeFalse();

            dictionary.Remove(MISSED_KEY).Should().BeFalse();
            assert(dictionary, 1);

            dictionary.Add(addKey, 123);
            assert(dictionary, 2);

            dictionary.Remove(validateKey).Should().BeTrue();
            assert(dictionary, 1);
        }
    }

    [Fact]
    public void IDictionaryRemove()
    {
        var listDictionary = _fixture.Create();
        var hashDictionary = _fixture.CreateWithDictionary();
        
        Check(listDictionary, MISSED_KEY, MISSED_KEY,(d, count) => d.AssertSwitchedToList(count));
        Check(hashDictionary, MISSED_KEY, MISSED_KEY, (d, count) => d.AssertSwitchedToDictionary(count));

        listDictionary = _fixture.Create(StringComparer.OrdinalIgnoreCase);
        hashDictionary = _fixture.CreateWithDictionary(StringComparer.OrdinalIgnoreCase);
        
        Check(listDictionary, MISSED_KEY, MISSED_KEY_ANOTHER_CASE,(d, count) => d.AssertSwitchedToList(count));
        Check(hashDictionary, MISSED_KEY, MISSED_KEY_ANOTHER_CASE, (d, count) => d.AssertSwitchedToDictionary(count));

        void Check(HybridDictionary<string, int> dictionary, string addKey, string validateKey, Action<HybridDictionary<string, int>, int> assert)
        {
            var iDictionary = (IDictionary<string, int>)dictionary;
            dictionary.AddItem();
            var item = dictionary.AddItem();
            assert(dictionary, 2);

            iDictionary.Remove(new KeyValuePair<string, int>(item.Key, int.MinValue)).Should().BeFalse();
            assert(dictionary, 2);

            iDictionary.Remove(new KeyValuePair<string, int>(item.Key, item.Value)).Should().BeTrue();
            assert(dictionary, 1);
            
            dictionary.ContainsKey(item.Key).Should().BeFalse();

            dictionary.Add(addKey, 123);
            assert(dictionary, 2);

            iDictionary.Remove(new KeyValuePair<string, int>(validateKey, 123)).Should().BeTrue();
            assert(dictionary, 1);
        }
    }
}

internal static class HybridDictionaryExtensions
{
    private static HybridDictionary<HybridDictionary<string, int>, int> _indexes = new HybridDictionary<HybridDictionary<string, int>, int>();
    
    internal static HybridDictionaryWrapper<TKey, TValue> ToWrapper<TKey, TValue>(this HybridDictionary<TKey, TValue> dictionary)
    {
        return new HybridDictionaryWrapper<TKey, TValue>(dictionary);
    }

    internal static ICollection<KeyValuePair<string, int>> AsCollection(this HybridDictionary<string, int> d) => d;

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

    internal static KeyValuePair<string, int> AddItem(this HybridDictionary<string, int> dictionary)
    {
        if (!_indexes.TryGetValue(dictionary, out int index))
        {
            index = 1;
            _indexes[dictionary] = index;
        }

        string key = index.ToString();
        dictionary.Add(key, index);
        _indexes[dictionary] += 1;

        return new KeyValuePair<string, int>(key, index);
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