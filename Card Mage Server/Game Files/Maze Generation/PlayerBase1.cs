using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Maze_Generation
{
    /// <summary>
    /// using
    /// TempMark1 as "border for submaze generation
    /// TempMark2 as "temporary walls", to be removed after everything else
    /// </summary>
    public class PlayerBase1 : PlayerBase
    {

        public override void GenerateBases(Map map, int player1Y, int player2Y)
        {
            int lastRow = map.length - 2;

            //------------player 1------------
            //row 0
            map.SetTile(new GridPosition(1, player1Y - 2), TileType.Traversible);
            map.SetTile(new GridPosition(1, player1Y - 1), TileType.Traversible);
            map.SetTile(new GridPosition(1, player1Y), TileType.Player1);
            map.SetTile(new GridPosition(1, player1Y + 1), TileType.Traversible);
            map.SetTile(new GridPosition(1, player1Y + 2), TileType.Traversible);
            //row 1
            map.SetTile(new GridPosition(2, player1Y - 1), TileType.Traversible);
            map.SetTile(new GridPosition(2, player1Y), TileType.Traversible);
            map.SetTile(new GridPosition(2, player1Y + 1), TileType.Traversible);
            map.SetTile(new GridPosition(2, player1Y + 2), TileType.Traversible);
            //row 2
            map.SetTile(new GridPosition(3, player1Y), TileType.Traversible);
            map.SetTile(new GridPosition(3, player1Y + 1), TileType.Traversible);
            map.SetTile(new GridPosition(3, player1Y + 2), TileType.Traversible);
            //row 3
            map.SetTile(new GridPosition(4, player1Y - 1), TileType.TempMark1);
            map.SetTile(new GridPosition(4, player1Y), TileType.TempMark1);
            map.SetTile(new GridPosition(4, player1Y + 3), TileType.TempMark1);
            map.SetTile(new GridPosition(4, player1Y + 4), TileType.TempMark1);

            //left wall (at least one of the three tiles is traversible)
            int rand = rng.Next(1, 7);
            for (int i = 1; i <= 3; i++)
            {
                map.SetTile(new GridPosition(i, player1Y - 3 + i - 1), (rand & (1 << (i - 1))) == 0 ? TileType.TempMark1 : TileType.TempMark2);
            }


            //right wall (at least one of the three tiles is traversible)
            rand = rng.Next(1, 7);
            for (int i = 1; i <= 3; i++)
            {
                map.SetTile(new GridPosition(i, player1Y + 3), (rand & (1 << (i - 1))) == 0 ? TileType.TempMark1 : TileType.TempMark2);
            }

            //front wall (at least one of the two tiles is traversible)
            int r1 = rng.Next(3);
            map.SetTile(new GridPosition(4, player1Y + 1), r1 <= 2 ? TileType.TempMark2 : TileType.TempMark1);
            map.SetTile(new GridPosition(4, player1Y + 2), r1 >= 2 ? TileType.TempMark2 : TileType.TempMark1);

            //-------------------player 2---------------
            //row 0
            map.SetTile(new GridPosition(lastRow, player2Y - 2), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow, player2Y - 1), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow, player2Y), TileType.Player2);
            map.SetTile(new GridPosition(lastRow, player2Y + 1), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow, player2Y + 2), TileType.Traversible);
            //row 1
            map.SetTile(new GridPosition(lastRow - 1, player2Y - 2), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow - 1, player2Y - 1), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow - 1, player2Y), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow - 1, player2Y + 1), TileType.Traversible);
            //row 2
            map.SetTile(new GridPosition(lastRow - 2, player2Y - 2), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow - 2, player2Y - 1), TileType.Traversible);
            map.SetTile(new GridPosition(lastRow - 2, player2Y), TileType.Traversible);
            //row 3
            map.SetTile(new GridPosition(lastRow - 3, player2Y - 4), TileType.TempMark1);
            map.SetTile(new GridPosition(lastRow - 3, player2Y - 3), TileType.TempMark1);
            map.SetTile(new GridPosition(lastRow - 3, player2Y), TileType.TempMark1);
            map.SetTile(new GridPosition(lastRow - 3, player2Y + 1), TileType.TempMark1);

            //left wall (at least one of the three tiles is traversible)
            rand = rng.Next(1, 7);
            for (int i = 0; i < 3; i++)
            {
                map.SetTile(new GridPosition(lastRow - i, player2Y - 3), (rand & (1 << i)) == 0 ? TileType.TempMark1 : TileType.TempMark2);
            }

            //right wall (at least one of the three tiles is traversible)
            rand = rng.Next(1, 7);
            for (int i = 0; i < 3; i++)
            {
                map.SetTile(new GridPosition(lastRow - i,player2Y + 3 - i), (rand & (1 << i)) == 0 ? TileType.TempMark1 : TileType.TempMark2);
            }

            //front wall (at least one of the two tiles is traversible)
            int r2 = rng.Next(3);
            map.SetTile(new GridPosition(lastRow - 3, player2Y - 1), r2 <= 2 ? TileType.TempMark2 : TileType.TempMark1);
            map.SetTile(new GridPosition(lastRow - 3, player2Y - 2), r1 >= 2 ? TileType.TempMark2 : TileType.TempMark1);
        }
    }
}
