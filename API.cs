using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Logic
{
    public static class API
    {
        public static Path Search(string source, string target, Line[] metros)
        {
            int sourceHash = source.GetHashCode();
            List<Path> shortestPaths = new List<Path>();

            foreach (Line metro in metros)
            {
                Station sourceStation;
                if (metro.Stations.TryGetValue(sourceHash, out sourceStation))
                {
                    //Task.Factory.StartNew(() => StartSearch(sourceStation, target, metros));
                    shortestPaths.Add(StartSearch(sourceStation, target, metros));
                }
            }

            if (shortestPaths.Count == 0)
            {
                return null;
            }

            Path shortestPath = shortestPaths[0];
            foreach (Path shortPath in shortestPaths)
            {
                if (shortPath != null && shortPath.LastDuration < shortestPath.LastDuration)
                { shortestPath = shortPath; }
            }

            return shortestPath;
        }

        private static Path StartSearch(Station source, string target, Line[] metros)
        {
            Dictionary<int, Path> visited = new Dictionary<int, Path>();
            List<DNeighbor> neighbors = new List<DNeighbor>();

            DNeighbor actStation = new DNeighbor(0, source, new Path(new int[] { }, new Station[] { }, 0, source));
            //Path actPath = new Path(new int[] { }, new Station[] { }, actStation.Duration, actStation.Station);

            do
            {
                foreach (Connection neighbor in actStation.Station.Connections)
                {
                    Station newNeighbor = null;
                    if (neighbor.Station1 != actStation.Station)
                    {
                        newNeighbor = neighbor.Station1;
                    }
                    else if (neighbor.Station2 != actStation.Station)
                    {
                        newNeighbor = neighbor.Station2;
                    }

                    if (newNeighbor != null)
                    {
                        Path tmpPathForNeighbor;
                        if (!visited.TryGetValue((newNeighbor.Name + newNeighbor.Metro.Name).GetHashCode(), out tmpPathForNeighbor))
                        {
                            int fullDuration = (neighbor.Duration + actStation.Duration);
                            Path newPath = new Path(actStation.Path.Durations, actStation.Path.StationPath, fullDuration, newNeighbor);
                            neighbors.Add(new DNeighbor(fullDuration, newNeighbor, newPath));
                        }
                    }
                }
                int actHash = (actStation.Station.Name + actStation.Station.Metro.Name).GetHashCode();
                if (!visited.ContainsKey(actHash))
                {
                    visited.Add(actHash, actStation.Path);
                }

                actStation = GetNearestNeighbor(neighbors);
                //actPath = new Path(actPath.Durations, actPath.StationPath, actStation.Duration, actStation.Station);
            } while (actStation != null && actStation.Station.Name != target);

            if (actStation == null)
            { return null; }

            return actStation.Path;
        }

        private static DNeighbor GetNearestNeighbor(List<DNeighbor> allNeighbors)
        {
            if (allNeighbors.Count == 0)
            { return null; }

            DNeighbor retVal = allNeighbors[0];
            foreach (DNeighbor neighbor in allNeighbors)
            {
                if (neighbor.Duration < retVal.Duration)
                { retVal = neighbor; }
            }
            allNeighbors.Remove(retVal);
            return retVal;
        }
    }
}
