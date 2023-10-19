public class MoveClass
{
    public uint  axis { get; private set; }
    public uint index {get; private set;}
    public int number;
    // Start is called before the first frame update
    public MoveClass(uint _axis, uint _index, int _number)
    {
        axis = _axis;
        index = _index;
        number = _number;
    }
}
