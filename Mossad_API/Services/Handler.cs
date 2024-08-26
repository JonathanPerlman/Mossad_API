using Mossad_API.Moddels.DBModdels;

namespace Mossad_API.Services
{
    public static class Handler
    {
        public static Double GetDistance(Location targetLocation, Location agentLocation)
        {
            return Math.Sqrt(Math.Pow(targetLocation.X - agentLocation.X, 2) + Math.Pow(targetLocation.Y - agentLocation.Y, 2));

        }


        public static Location CalculateLocation(Location location, string direction)
        {
            Location _location = new Location();

            switch (direction)
            {
                case "nw":
                    _location.X = location.X -1;
                    _location.Y = location.Y + 1;
                    break;
                case "n":
                    _location.X = location.X;
                    _location.Y = location.Y + 1;
                    break;
                case "ne":
                    _location.X = location.X + 1;
                    _location.Y = location.Y + 1;
                    break;
                case "w":
                    _location.X = location.X - 1;
                    _location.Y = location.Y;   
                    break;
                case "e":
                    _location.X = location.X + 1;
                    _location.Y = location.Y;
                    break;
                case "sw":
                    _location.X = location.X - 1;
                    _location.Y = location.Y - 1 ;
                    break;
                case "s":
                    _location.X = location.X;
                    _location.Y = location.Y - 1;
                    break;
                case "se":
                    _location.X = location.X + 1;
                    _location.Y = location.Y - 1;
                    break;
            }
            return _location;
        }




        public static string CalculateDirection(Agent agent, Target target)
        {
            if (agent._Location.X > target._Location.X && agent._Location.Y > target._Location.Y)
            {
                return "nw";
            }

            if (agent._Location.X == target._Location.X && agent._Location.Y < target._Location.Y)
            {
                return "n";
            }
            if (agent._Location.X < target._Location.X && agent._Location.Y < target._Location.Y)
            {
                return "ne";
            }
            if (agent._Location.Y == target._Location.Y && agent._Location.X > target._Location.X)
            {
                return "w";
            }
            if (agent._Location.Y == target._Location.Y && agent._Location.X < target._Location.X)
            {
                return "e";
            }
            if (agent._Location.X > target._Location.X && agent._Location.Y > target._Location.Y)
            {
                return "sw";
            }
            if (agent._Location.X == target._Location.X && agent._Location.Y > target._Location.Y)
            {
                return "s";
            }
            if (agent._Location.X < target._Location.X && agent._Location.Y > target._Location.Y)
            {
                return "se";
            }

            return "";
        }
    }
}











