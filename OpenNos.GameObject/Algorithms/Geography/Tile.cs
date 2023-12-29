namespace OpenNos.GameObject.Algorithms.Geography
{
    public struct Tile
    {
        public byte Value { get; set; }
        public short X { get; set; }

        public short Y { get; set; }
        
        public short Z { get; set; }

        public Tile(short x, short y, byte value)
        {
            X = x;
            Y = y;
            Value = value;
            Z = 0;
        }

        public Tile Translate(short x, short y)
        {
            return new Tile((short)(X + x), (short)(Y + y), Value);
        }
    }
}