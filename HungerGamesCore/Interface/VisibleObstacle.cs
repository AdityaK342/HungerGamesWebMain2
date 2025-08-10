using DongUtility;
using Geometry.Geometry2D;
using HungerGamesCore.Terrain;
using System;

namespace HungerGamesCore.Interface
{
    public enum ObstacleType { Tree, Shrub, Grass, Water };

    public class VisibleObstacle
    {
        internal Obstacle Obstacle;

        public VisibleObstacle(Obstacle obstacle)
        {
            Obstacle = obstacle;
        }

        public ObstacleType ObstacleType
        {
            get
            {
                if (Obstacle is Tree)
                    return ObstacleType.Tree;
                else if (Obstacle is Shrub)
                    return ObstacleType.Shrub;
                else if (Obstacle is Grass)
                    return ObstacleType.Grass;
                else if (Obstacle is Water)
                    return ObstacleType.Water;
                else
                    throw new NotSupportedException("Invalid PlantType used!");
            }
        }

        public Point Position => Obstacle.Position;
        public Shape2D Size => Obstacle.Shape;
        public double Height
        {
            get
            {
                if (Obstacle is Grass grass)
                {
                    return grass.Height;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
