using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    private const int MINIMAM_RANGE_WIDTH = 6;

    private int mapSizeX;
    private int mapSizeY;
    private int maxRoom;
    
    private List<Range> roomList = new List<Range>();
    private List<Range> passList = new List<Range>();
    private List<Range> rangeList = new List<Range>();
    private List<Range> roomPassList = new List<Range>();

    private bool isGenerated = false;

    public int[,] GenerateMap(int mapSizeX, int mapSizeY, int maxRoom)
    {
        this.mapSizeX = mapSizeX;
        this.mapSizeY = mapSizeY;
        
        int[,] map = new int[mapSizeX, mapSizeY];

        CreateRange(maxRoom);
        CreateRoom();

        foreach (Range pass in passList)
        {
            for (int x = pass.Start.X; x <= pass.End.X; x++)
            {
                for (int y = pass.Start.X; y <= pass.End.Y; y++)
                {
                    map[x, y] = 1;
                }
            }
        }

        foreach (Range roomPass in roomPassList)
        {
            for (int x = roomPass.Start.X; x <= roomPass.End.X; x++)
            {
                for (int y = roomPass.Start.Y; y <= roomPass.End.Y; y++)
                {
                    map[x, y] = 1;
                }
            }
        }

        foreach (Range room in roomList)
        {
            for (int x = room.Start.X; x <= room.End.X; x++)
            {
                for (int y = room.Start.Y; y <= room.End.Y; y++)
                {
                    map[x, y] = 1;
                }
            }
        }

        TrimPassList(ref map);

        return map;
    }

    public void CreateRange(int maxRoom)
    {
        rangeList.Add(new Range(0,0,mapSizeX - 1, mapSizeY -1));

        bool isDevided;
        do
        {
            isDevided = DevideRange(false);
            isDevided = DevideRange(true) || isDevided;

            if (rangeList.Count >= maxRoom)
            {
                break;
            }
        } while (isDevided);
    }

    public bool DevideRange(bool isVertical)
    {
        bool isDevided = false;
        
        List<Range> newRangeList = new List<Range>();
        foreach (Range range in rangeList)
        {
            if (isVertical && range.GetWidthY() < MINIMAM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }else if (!isVertical && range.GetWidthX() < MINIMAM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }
            
            System.Threading.Thread.Sleep(1);

            if (rangeList.Count > 1 && RogueUtils.RandomJadge(0.4f))
            {
                continue;
            }

            int length = isVertical ? range.GetWidthY() : range.GetWidthX();
            int margin = length - MINIMAM_RANGE_WIDTH * 2;
            int baseIndex = isVertical ? range.Start.Y : range.Start.X;
            int devideIndex = baseIndex + MINIMAM_RANGE_WIDTH + RogueUtils.GetRandomInt(1, margin) - 1;
            
            Range newRange = new Range();
            if (isVertical)
            {
                passList.Add(new Range(range.Start.X,devideIndex,range.End.X,devideIndex));
                newRange = new Range(range.Start.X,devideIndex + 1, range.End.X,range.End.Y);
                range.End.Y = devideIndex - 1;
            }
            else
            {
                passList.Add(new Range(devideIndex,range.Start.Y, devideIndex, range.End.Y));
                newRange = new Range(devideIndex + 1, range.Start.Y,range.End.X,range.End.Y);
                range.End.X = devideIndex - 1;
            }
            
            newRangeList.Add(newRange);

            isDevided = true;
        }
        
        rangeList.AddRange(newRangeList);

        return isDevided;
    }

    private void CreateRoom()
    {
        rangeList.Sort((a,b) => RogueUtils.GetRandomInt(0,1) - 1);

        foreach (Range range in rangeList)
        {
            System.Threading.Thread.Sleep(1);
            if (roomList.Count > maxRoom / 2 && RogueUtils.RandomJadge(0.3f))
            {
                continue;
            }

            int marginX = range.GetWidthX() - MINIMAM_RANGE_WIDTH;
            int marginY = range.GetWidthY() - MINIMAM_RANGE_WIDTH;

            int randomX = RogueUtils.GetRandomInt(1, marginX);
            int randomY = RogueUtils.GetRandomInt(1, marginY);

            int startX = range.Start.X + randomX;
            int endX = range.End.X - RogueUtils.GetRandomInt(0, (marginX - randomX)) - 1;
            int startY = range.Start.Y + randomY;
            int endY = range.End.Y - RogueUtils.GetRandomInt(0, (marginY - randomY)) - 1;
            
            Range room = new Range(startX,startY,endX,endY);
            roomList.Add(room);

            CreatePass(range, room);
        }
    }

    private void CreatePass(Range range, Range room)
    {
        List<int> directionList = new List<int>();
        if (range.Start.X != 0)
        {
            directionList.Add(0);
        }

        if (range.End.X != mapSizeX - 1)
        {
            directionList.Add(1);
        }

        if (range.Start.Y != 0)
        {
            directionList.Add(2);
        }

        if (range.End.Y != mapSizeY - 1)
        {
            directionList.Add(3);
        }
        
        directionList.Sort((a,b) => RogueUtils.GetRandomInt(0,1) - 1);

        bool isFirst = true;
        foreach (int direction in directionList)
        {
            System.Threading.Thread.Sleep(1);

            if (!isFirst && RogueUtils.RandomJadge(0.8f))
            {
                continue;
            }
            else
            {
                isFirst = false;
            }

            int random;
            switch (direction)
            {
                case 0 :
                    random = room.Start.Y + RogueUtils.GetRandomInt(1, room.GetWidthY()) - 1;
                    roomPassList.Add(new Range(range.Start.X, random, room.Start.X - 1, random));
                    break;
                case 1:
                    random = room.Start.Y + RogueUtils.GetRandomInt(1, room.GetWidthY()) - 1;
                    roomPassList.Add(new Range(room.End.X + 1, random, range.End.X, random));
                    break;
                case 2:
                    random = room.Start.X + RogueUtils.GetRandomInt(1, room.GetWidthX()) - 1;
                    roomPassList.Add(new Range(random, range.Start.Y, random, room.Start.Y - 1));
                    break;
                case 3:
                    random = room.Start.X + RogueUtils.GetRandomInt(1, room.GetWidthX()) - 1;
                    roomPassList.Add(new Range(random, room.End.Y + 1, random, range.End.Y));
                    break;
            }
        }
    }

    private void TrimPassList(ref int[,] map)
    {
        for (int i = passList.Count - 1; i >= 0; i++)
        {
            Range pass = passList[i];

            bool isVertical = pass.GetWidthY() > 1;

            bool isTrimTarget = true;
            if (isVertical)
            {
                int x = pass.Start.X;
                for (int y = pass.Start.Y; y <= pass.End.Y; y++)
                {
                    if (map[x - 1, y] == 1 || map[x + 1, y] == 1)
                    {
                        isTrimTarget = false;
                        break;
                    }
                }
            }
            else
            {
                int y = pass.Start.Y;
                for (int x = pass.Start.X; x <= pass.End.X; x++)
                {
                    if (map[x, y - 1] == 1 || map[x, y + 1] == 1)
                    {
                        isTrimTarget = false;
                        break;
                    }
                }
            }

            if (isTrimTarget)
            {
                passList.Remove(pass);

                if (isVertical)
                {
                    int x = pass.Start.X;
                    for (int y = pass.Start.Y; y <= pass.End.Y; y++)
                    {
                        map[x, y] = 0;
                    }
                }
                else
                {
                    int y = pass.Start.Y;
                    for (int x = pass.Start.X; x <= pass.End.X; x++)
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }

        for (int x = 0; x < mapSizeX - 1; x++)
        {
            if (map[x, 0] == 1)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    if (map[x - 1, y] == 1 || map[x + 1, y] == 1)
                    {
                        break;
                    }
                    map[x, y] = 0;
                }
            }

            if (map[x, mapSizeY - 1] == 1)
            {
                for (int y = mapSizeY - 1; y >= 0; y--)
                {
                    if (map[x - 1, y] == 1 || map[x + 1, y] == 1)
                    {
                        break;
                    }
                    map[x, y] = 0;
                }
            }
        }

        for (int y = 0; y < mapSizeY - 1; y++)
        {
            if (map[0, y] == 1)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    if (map[x, y - 1] == 1 || map[x, y + 1] == 1)
                    {
                        break;
                    }

                    map[x, y] = 0;
                }
            }

            if (map[mapSizeX - 1, y] == 1)
            {
                for (int x = mapSizeX - 1; x >= 0; x--)
                {
                    if (map[x, y - 1] == 1 || map[x, y + 1] == 1)
                    {
                        break;
                    }

                    map[x, y] = 0;
                }
            }
        }
    }
}
