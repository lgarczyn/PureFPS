using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : Singleton<TeamManager>
{

    public const int TEAM_COUNT = 2;

    static public HashSet<IAffectable>[] teams = new HashSet<IAffectable>[TEAM_COUNT];
}
