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
    //Care with this one
    public void ChangeAxis(uint _axis)
    {
        axis = _axis;
    }
    //Care with this one
    public void ChangeNumber(int _number)
    {
        number = _number;
    }
}
