using System.Collections.Generic;

[System.Serializable]
public class BossPhase
{
    public string phaseName;                        // 에디터에서 확인 할 용도   
    public float speedMultiplier = 1.0f;            // 패턴 속도 배율

    public List<BossPattern> shortPattern = new ();    // 근거리 패턴
    public List<BossPattern> middlePattern = new ();   // 중거리 패턴
    public List<BossPattern> longPattern = new ();     // 장거리 패턴

}
