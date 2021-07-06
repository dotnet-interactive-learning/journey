using Microsoft.DotNet.Interactive.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// will fix
//using Microsoft.DotNet.Interactive.VSCode;

namespace Extension
{
    public class CellController
    {
        private SortedDictionary<string, List<HiddenCell>> hiddenCells;

        public CellController()
        {
            hiddenCells = new SortedDictionary<string, List<HiddenCell>>();
        }

        public void AddHiddenCell(string hiddenId, HiddenCell hiddenCell)
        {
            if (!hiddenCells.ContainsKey(hiddenId))
            {
                hiddenCells.Add(hiddenId, new List<HiddenCell>());
            }
            hiddenCells[hiddenId].Add(hiddenCell);
        }

        public IEnumerable<AddCell> GetAddCellCommands(string hiddenId)
        {
            if (!hiddenCells.ContainsKey(hiddenId))
            {
                return Enumerable.Empty<AddCell>();
            }
            return hiddenCells[hiddenId].Select(hiddenCell => new AddCell("csharp", hiddenCell.ToCode()));
        }
    }

    // will remove
    public class AddCell : KernelCommand
    {
        public string Language { get; }
        public string Contents { get; }

        public AddCell(string language, string contents, string targetKernelName = null)
            : base(targetKernelName)
        {
            Language = language;
            Contents = contents;
        }
    }
}
