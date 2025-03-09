using BBP_Playables.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBP_Playables.Modded.BCPP;

public class DwellerComponent : PlayableCharacterComponent
{
    public Map Map { get; private set; }
    private List<Cell> cells;
    protected override void Start()
    {
        base.Start();
        Map = pm.ec.map;
        cells = pm.ec.AllCells();
        foreach (var item in pm.ec.items)
            if (item.icon != null && item.item.itemType != Items.None)
                item.AssignItem(item.item);
    }
    void Update()
    {
        if (Map == null || cells == null) return;
        foreach (var cell in cells)
        {
            if (cell == null)
            {
                cells.Remove(cell);
                continue;
            }
            if (!cell.hideFromMap && !Map.foundTiles[cell.position.x, cell.position.z] && (cell.CenterWorldPosition - transform.position).magnitude < 40f)
            {
                if (cell.room.type == RoomType.Hall)
                    Map.Find(cell.position.x, cell.position.z, cell.ConstBin, cell.room);
                else
                    foreach (var roomcell in cell.room.cells.Where(_cell => !_cell.hideFromMap))
                        Map.Find(roomcell.position.x, roomcell.position.z, roomcell.ConstBin, cell.room);
            }
        }
    }
}

