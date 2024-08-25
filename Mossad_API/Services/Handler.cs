using Mossad_API.Moddels;

namespace Mossad_API.Services
{
    public static class Handler
    {
        public static Double GetDistance(Location targetLocation, Location agentLocation)
        {
            return Math.Sqrt(Math.Pow(targetLocation.x - agentLocation.x, 2) + Math.Pow(targetLocation.y - agentLocation.y, 2));

        }


        public static Location CalculateLocation(Location location, string direction)
        {
            Location _location = new Location();

            switch (direction)
            {
                case "nw":
                    _location.x = location.x -1;
                    _location.y = location.y + 1;
                    break;
                case "n":
                    _location.x = location.x;
                    _location.y = location.y + 1;
                    break;
                case "ne":
                    _location.x = location.x + 1;
                    _location.y = location.x + 1;
                    break;
                case "w":
                    _location.x = location.x - 1;
                    _location.y = location.y;   
                    break;
                case "e":
                    _location.x = location.x + 1;
                    _location.y = location.y;
                    break;
                case "sw":
                    _location.x = location.x - 1;
                    _location.y = location.y - 1 ;
                    break;
                case "s":
                    _location.x = location.x;
                    _location.y = location.y - 1;
                    break;
                case "se":
                    _location.x = location.x + 1;
                    _location.y = location.y - 1;
                    break;
            }
            return _location;
        }




        public static string CalculateDirection(Agent agent, Target target)
        {
            if (agent._Location.x > target._Location.x && agent._Location.y > target._Location.y)
            {
                return "nw";
            }

            if (agent._Location.x == target._Location.x && agent._Location.y < target._Location.y)
            {
                return "n";
            }
            if (agent._Location.x < target._Location.y && agent._Location.y < target._Location.y)
            {
                return "ne";
            }
            if (agent._Location.y == target._Location.y && agent._Location.x > target._Location.x)
            {
                return "w";
            }
            if (agent._Location.y == target._Location.y && agent._Location.x < target._Location.x)
            {
                return "e";
            }
            if (agent._Location.x > target._Location.x && agent._Location.y > target._Location.y)
            {
                return "sw";
            }
            if (agent._Location.x == target._Location.x && agent._Location.y > target._Location.y)
            {
                return "s";
            }
            if (agent._Location.x < target._Location.x && agent._Location.y > target._Location.y)
            {
                return "se";
            }

            return "";
        }
    }
}











