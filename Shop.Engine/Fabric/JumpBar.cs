using Commune.Basis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Engine
{
	public class JumpBar
	{
		public readonly int AllItemCount;
		public readonly int ItemOnPageCount;
		public readonly int PageCount;
		public readonly int PageIndex;
		public readonly int MorePageCount;

		public JumpBar(int allItemCount, int itemOnPageCount, int pageIndex, int morePageCount)
		{
			this.AllItemCount = allItemCount;
			this.ItemOnPageCount = itemOnPageCount;
			this.PageCount = BinaryHlp.RoundUp(allItemCount, itemOnPageCount);
			if (pageIndex < 0 || pageIndex >= PageCount)
				pageIndex = 0;
			this.PageIndex = pageIndex;
			this.MorePageCount = morePageCount;
		}

		public T[] GetShowItems<T>(IList<T> allItems)
		{
			if (PageCount < 2)
				return allItems.ToArray();

			int curPos = PageIndex * ItemOnPageCount;
			if (curPos < 0 || curPos >= allItems.Count)
				curPos = 0;

			int pageCount = ItemOnPageCount * (1 + MorePageCount);
			return _.GetRange<T>(allItems, curPos, Math.Min(pageCount, allItems.Count - curPos));
		}

		public bool PrevPossibly
		{
			get { return PageIndex > 0; }
		}

		public bool NextPossibly
		{
			get { return PageIndex < PageCount - 1; }
		}

		public bool MorePossibly
		{
			get { return PageIndex + MorePageCount < PageCount - 1; }
		}

		public bool IsShowPage(int pageIndex)
		{
			return pageIndex >= PageIndex && pageIndex <= PageIndex + MorePageCount;
		}

		public int?[] GetShowJumpElementIndices(int shift)
		{
			List<int?> indices = new List<int?>();
			if (PageCount <= shift * 2 + 5)
			{
				for (int i = 0; i < PageCount; ++i)
					indices.Add(i);
				return indices.ToArray();
			}

			int startIndex = Math.Max(0, PageIndex - shift);

			if (startIndex != 0)
			{
				indices.Add(0);
				if (startIndex > 1)
					indices.Add(null);
			}

			int endIndex = Math.Min(PageCount - 1, startIndex + shift * 2 + 2 - indices.Count);

			for (int i = startIndex; i <= endIndex; ++i)
				indices.Add(i);

			if (endIndex < PageCount - 1)
			{
				if (endIndex < PageCount - 2)
					indices.Add(null);
				indices.Add(PageCount - 1);
			}

			return indices.ToArray();
		}
	}
}
