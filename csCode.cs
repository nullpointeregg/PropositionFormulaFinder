using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
    public static void Main()
    {

        var _outputs = new int[] { 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1  };
        var _set = Proposition.Calculate(_outputs);
        string _formula = Proposition.GetFormula(_set);

        Console.WriteLine(_formula);
    }
}

class Proposition
{
    readonly static int[] SIZE_LIST = { 1, 2, 4, 8, 16, 32, 64, 128, 256 };

    public static HashSet<(IEnumerable<int>, bool[])> 
        Calculate(bool[] _outputs)
    {
        int _size = Array.FindIndex(SIZE_LIST, x => x == _outputs.Length);

        if (_size == -1)
            return new HashSet<(IEnumerable<int>, bool[])>();
        HashSet<int> _indexes = Enumerable.Range(0, _outputs.Length).Where(x => _outputs[x]).ToHashSet();
        var _set = new HashSet<(IEnumerable<int>, bool[])>();

        while (_indexes.Any())
        {
            int _target = _indexes.First();
            bool _found = false;

            for (int i = 0; i < _size && !_found; i++)
            {
                var _items = GetKCombs(Enumerable.Range(0, _size), i + 1);
                int _itemsCount = _items.Count();
                for (int j = 0; j < _itemsCount && !_found; j++)
                {
                    bool _isAllTrue = Enumerable
                        .Range(0, _outputs.Length)
                        .Where(num => _items.ElementAt(j).All(item => (num >> item & 1) == (_target >> item & 1)))
                        .All(x => _outputs[x]);

                    if (_isAllTrue)
                    {
                        var _tuple = (_items.ElementAt(j).Select(x => _size - x), _items.ElementAt(j).Select(item => (_target >> item & 1) == 1).ToArray());
                        _set.Add(_tuple);
                        _found = true;

                        _indexes.RemoveWhere(x => _items.ElementAt(j).All(item => (x >> item & 1) == (_target >> item & 1)));
                        break;
                    }
                }
            }

            _found = false;
        }

        Trim(_set, _size);
        return _set;
    }

    public static HashSet<(IEnumerable<int>, bool[])>
        Calculate(int[] _outputs)
    {
        return Calculate(_outputs.Select(o => o != 0).ToArray());
    }

    public static string GetFormula(HashSet<(IEnumerable<int>, bool[])> _set)
    {
        StringBuilder _sb = new StringBuilder();

        for (int i = 0; i < _set.Count(); i++)
        {
            var _tuple = _set.ElementAt(i);
            for (int j = 0; j < _tuple.Item1.Count(); j++)
            {
                _sb.Append($"p{_tuple.Item1.ElementAt(j)}{(_tuple.Item2.ElementAt(j) ? "" : "'")}");
            }
            if (i < _set.Count() - 1)
                _sb.Append(" + ");
        }

        return _sb.ToString();
    }

    public static void Trim(HashSet<(IEnumerable<int>, bool[])> _set, int _size)
    {
        int _sizeTo2Ex = (int)Math.Pow(2, _size);
        var _countArray = new int[_sizeTo2Ex];
        var _dictionary = new Dictionary<(IEnumerable<int>, bool[]), int[]>();

        foreach (var _tuple in _set)
        {
            int _itemsCount = _tuple.Item1.Count();
            bool[] _exclusiveSaves = new bool[_sizeTo2Ex];
            for (int i = 0; i < _sizeTo2Ex; i++)
                _exclusiveSaves[i] = true;

            for (int i = 0; i < _itemsCount; i++)
            {
                int _item1 = _tuple.Item1.ElementAt(i);
                bool _isTrue = _tuple.Item2.ElementAt(i);
                int[] _positions = Enumerable.Range(0, _sizeTo2Ex)
                    .Where(x => (x >> (_size - _item1) & 1) != (_isTrue ? 1 : 0))
                    .ToArray();
                Array.ForEach(_positions, n => _exclusiveSaves[n] = false);

            }
            _dictionary.Add(_tuple, Enumerable.Range(0, _sizeTo2Ex).Where(x => _exclusiveSaves[x]).ToArray());
            for (int i = 0; i < _sizeTo2Ex; i++)
            {
                if (_exclusiveSaves[i])
                {
                    _countArray[i]++;
                }
            }
        }

        bool _isTrimmed = false;
        do
        {
            var _redundant = _set.FirstOrDefault(t => _dictionary[t].All(x => _countArray[x] != 1));
            if (_redundant.Item1 == null)
                _isTrimmed = true;
            else
                _set.Remove(_redundant);
        } while (!_isTrimmed);
    }

    static IEnumerable<IEnumerable<T>>
        GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
    {
        if (length == 1) return list.Select(t => new T[] { t });
        return GetKCombs(list, length - 1)
            .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                (t1, t2) => t1.Concat(new T[] { t2 }));
    }
}

// 불필요한 계산 줄이기
// 함소 호출 줄이기
// 가비지 생성 최소화하기
// 루프 줄이기
