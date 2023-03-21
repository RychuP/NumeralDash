namespace NumeralDash.Entities;

class FastMove
{
    Direction _direction = Direction.None;
    bool _isOn = false;

    public void Reset() =>
        Stop();

    public void Start(Direction direction)
    {
        if (direction == Direction.None)
            throw new ArgumentException("Invalid start direction.");
        Direction = direction;
        IsOn = true;
    }

    public void Stop()
    {
        Direction = Direction.None;
        IsOn = false;
    }

    public void ChangeDirection(Direction direction) =>
        Direction = direction;

    public Direction Direction
    {
        get => _direction;
        set
        {
            if (value != Direction.None && !value.IsCardinal())
                throw new ArgumentException("Invalid direction.");
            _direction = value;
        }
    }

    public bool IsOn
    {
        get => _isOn;
        set
        {
            bool prevValue = _isOn;
            _isOn = value;
            OnIsOnChanged(prevValue, value);
        }
    }

    void OnIsOnChanged(bool prevValue, bool newValue)
    {
        if (prevValue == true && newValue == false)
            Stopped?.Invoke(this, EventArgs.Empty);
        else if (prevValue == false && newValue == true)
            Started?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Started;
    public event EventHandler? Stopped;
}