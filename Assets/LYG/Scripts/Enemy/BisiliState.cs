using UnityEngine;

public abstract class BisiliState
{
    public abstract void Start(Bisili bs);
    public abstract void Update(Bisili bs);
    public abstract void Exit(Bisili bs);
}

public class BSBattleIdleState : BisiliState
{
    
    public override void Start(Bisili bs)
    {
        
    }

    public override void Update(Bisili bs)
    {
        bs.SpriteControl();
    }

    public override void Exit(Bisili bs)
    {
        
    }
}

public class BSChasingState : BisiliState
{

    public override void Start(Bisili bs)
    {

    }

    public override void Update(Bisili bs)
    {
        bs.SpriteControl();
    }

    public override void Exit(Bisili bs)
    {

    }
}

public class BSSwayState : BisiliState
{

    public override void Start(Bisili bs)
    {
        bs.SpriteControl();
    }

    public override void Update(Bisili bs)
    {

    }

    public override void Exit(Bisili bs)
    {

    }
}

public class BSAttackState : BisiliState
{

    public override void Start(Bisili bs)
    {

    }

    public override void Update(Bisili bs)
    {

    }

    public override void Exit(Bisili bs)
    {

    }
}

public class BSHitState : BisiliState
{

    public override void Start(Bisili bs)
    {

    }

    public override void Update(Bisili bs)
    {

    }

    public override void Exit(Bisili bs)
    {

    }
}

public class BSDeathState : BisiliState
{

    public override void Start(Bisili bs)
    {

    }

    public override void Update(Bisili bs)
    {

    }

    public override void Exit(Bisili bs)
    {

    }
}