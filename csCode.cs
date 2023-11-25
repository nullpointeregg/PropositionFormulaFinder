using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    public static void Main()
    {

        var _outputs = new int[] { 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 1 };
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
                for (int j = 0; j < _items.Count() && !_found; j++)
                {
                    bool _isAllTrue = Enumerable
                        .Range(0, _outputs.Length)
                        .Where(num => _items.ElementAt(j).All(item => (num & (1 << item)) == (_target & (1 << item))))
                        .All(x => _outputs[x]);

                    if (_isAllTrue)
                    {
                        var _tuple = (_items.ElementAt(j).Select(x => _size - x), _items.ElementAt(j).Select(item => (_target >> item & 1) == 1).ToArray());
                        _set.Add(_tuple);
                        _found = true;

                        _indexes.RemoveWhere(x => _items.ElementAt(j).All(item => (x & (1 << item)) == (_target & (1 << item))));
                        break;
                    }
                }
            }

            _found = false;
        }

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
