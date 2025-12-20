using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBossPhase", menuName = "Boss/Phase")]
public class BossPhase : ScriptableObject
{
    [Header("Phase Info")]
    public string phaseName;                        // 에디터에서 확인 할 용도   

    [Header("Stats Modifiers")]
    [Tooltip("이 페이즈의 패턴 속도 배율 (애니메이션 속도)")]
    public float speedMultiplier = 1.0f;
    [Tooltip("패턴 사이의 휴식 시간 (낮을수록 공격적)")]
    public float restTime = 1.0f;
    [Tooltip("이 페이즈의 강인도 (다 깎이면 그로기)")]
    public float maxPoise = 100f;

    [Header("Transition")]
    [Tooltip("페이즈 시작 시 무조건 1회 실행할 패턴 (포효, 기믹 등)")]
    public BossPattern entryPattern;

    [Header("Available Patterns")]
    [Tooltip("이 페이즈에서 사용 가능한 모든 패턴을 넣으세요")]
    public List<BossPattern> availablePatterns = new();
}