using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NumeralDash.Screens.TopSideWindow;

// mid section of the level animation showing the level number
class LevelNumber : ScreenSurface
{
    const char Dot = '.';
    const char Glyph = (char)254;
    readonly LinkedList<Point> _points = new();
    LinkedListNode<Point>? _currentNode = null;

    public LevelNumber(int number) : base(14, 12)
    {
        Position = (7, 4);

        // read the blueprint file
        var path = Path.Combine("Resources", "Text", $"{number}.txt");
        var text = File.ReadAllText(path);
        var eol = text.Contains("\r\n") ? "\r\n" : "\n";
        var lines = text.Split(eol);

        // check the lines
        if (lines.Length != Surface.Height || lines.Any(l => l.Length != Surface.Width))
            throw new ArgumentException("Text file width or height invalid.");

        // transfer all letters to an array for easy editing
        bool[,] markers = new bool[Surface.Height, Surface.Width];
        for (int y = 0; y < Surface.Height; y++)
            for (int x = 0; x < Surface.Width; x++)
                markers[y, x] = lines[y][x] == Dot;

        // find the first dot and create a point
        var startPoint = FindStartingPoint(markers);
        Point endPoint = (0, 0);
        if (startPoint is null)
            throw new ArgumentException("Text file didn't contain any points.");
        else
        {
            var (x, y) = endPoint = startPoint.Value;
            markers[y, x] = false;
            _points.AddFirst(endPoint);
        }

        Direction searchDirection = Direction.Right;
        bool chainIsComplete = false;
        int loopCount = 0;

        //create a chain of connected points
        do
        {
            if (_points.Last is null)
                throw new InvalidOperationException("Last is null. This should not happen.");

            // save the count of points to be able to check if the loop resulted in creating a node
            int pointCount = _points.Count;

            // get the neighbours of the last point in the search direction
            var neighbours = Neighbours(_points.Last.Value, searchDirection);

            // find the next chain point amongst the neighbours
            foreach (var neighbour in neighbours)
            {
                // discard points that are not within the bounds of the surface
                if (!Surface.Area.Contains(neighbour))
                    continue;

                // check if the point is a marker
                else if (markers[neighbour.Y, neighbour.X])
                {
                    // erase the marker
                    markers[neighbour.Y, neighbour.X] = false;

                    // add the point to the chain
                    _points.AddLast(neighbour);

                    // change search direction
                    searchDirection = _points.Last.Previous is null ? Direction.Right :
                        Direction.GetCardinalDirection(_points.Last.Previous.Value, _points.Last.Value);

                    // stop the loop
                    break;
                }

                // check if the loop is complete
                else if (_points.Count > 2 && neighbour == endPoint)
                {
                    // look for the second loop
                    startPoint = FindStartingPoint(markers);
                    if (startPoint is null)
                        chainIsComplete = true;
                    else
                    {
                        var (x, y) = endPoint = startPoint.Value;
                        markers[y, x] = false;
                        _points.AddLast(new Point(x, y));
                    }
                    
                    break;
                }
            }

            if (!chainIsComplete)
            {
                // check if a new node has been created
                if (pointCount == _points.Count)
                    break;
                //throw new ArgumentException("Text file invalid. Points either don't connect or don't form a loop.");

                // infinite loop check
                if (++loopCount == markers.Length)
                    throw new ArgumentException("Text file invalid. Loop count reached the number of markers " +
                        "and the chain is still not finished.");
            }

        } while (!chainIsComplete);
    }

    // finds the top left most point 
    Point? FindStartingPoint(bool[,] markers)
    {
        for (int y = 0; y < Surface.Height; y++)
            for (int x = 0; x < Surface.Width; x++)
                if (markers[y, x])
                    return new Point(x, y);
        return null;
    }

    // returns directions nearest to the search direction
    static IEnumerable<Direction> NeighbourSearchDirections(Direction direction)
    {
        yield return direction;
        yield return direction - 2;
        yield return direction + 2;
        yield return direction - 1;
        yield return direction + 1;
        yield return direction - 3;
        yield return direction + 3;
    }

    static IEnumerable<Point> Neighbours(Point point, Direction searchDirection)
    {
        foreach (var direction in NeighbourSearchDirections(searchDirection))
            yield return point + direction;
    }

    public void Reset()
    {
        if (_points.First is null)
            throw new InvalidOperationException("Point do not contain a valid start point.");
        Surface.Clear();
        _currentNode = _points.First;
    }

    public bool Animate()
    {
        if (_currentNode is null)
            throw new InvalidOperationException("Current node is null. Execution shouldn't get this far.");

        Surface.Print(_currentNode.Value, Glyph);
        _currentNode = _currentNode.Next;
        return _currentNode == null;
    }
}