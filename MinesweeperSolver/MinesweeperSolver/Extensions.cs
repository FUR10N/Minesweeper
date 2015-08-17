using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperSolver
{
	public static class Extensions
	{
		public static TSource MaxItem<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			bool initial = true;
			TSource maxItem = default(TSource);
			int max = 0;
			foreach (var item in source)
			{
				if (initial)
				{
					maxItem = item;
					max = selector(item);
					initial = false;
				}
				else
				{
					int temp = selector(item);
					if (temp > max)
					{
						maxItem = item;
						max = temp;
					}
				}
			}
			return maxItem;
		}

		public static TSource MinItem<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			bool initial = true;
			TSource minItem = default(TSource);
			int min = 0;
			foreach (var item in source)
			{
				if (initial)
				{
					minItem = item;
					min = selector(item);
					initial = false;
				}
				else
				{
					int temp = selector(item);
					if (temp < min)
					{
						minItem = item;
						min = temp;
					}
				}
			}
			return minItem;
		}
	}
}
