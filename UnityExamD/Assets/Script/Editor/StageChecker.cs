using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

//GameScene を再生するとき、ステージに問題がないかを自動で点検する
//遊びが成り立たない問題（迷路の外・壁の中・星が無いなど）があれば、日本語の画面で知らせて再生を止める
//難しくなるだけの問題（敵がスタート地点に近い）だけなら、「このまま遊ぶ」か「やめる」を選ばせる
//点検の中身は Check() に分けてある（画面を出さずに試せるように）
[InitializeOnLoad]
public static class StageChecker
{
    const string ScenePath = "Assets/GameScene.unity";
    const string MapName = "Map";
    const float NearStartDistance = 4f; //敵がスタート地点にこれより近いと、始めてすぐ捕まる
    const float DotReach = 1.2f;        //星の中心からこの距離まで近づければ取れる
    const int MaxLines = 8;             //画面に並べる問題の数

    public class Problem
    {
        public string Message;
        public GameObject Target;
        public bool Blocking; //true なら遊べないので再生させない
        public Problem(string message, GameObject target, bool blocking = true) { Message = message; Target = target; Blocking = blocking; }
    }

    static StageChecker()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) { return; }
        if (EditorSceneManager.GetActiveScene().path != ScenePath) { return; }
        var problems = Check();
        if (problems.Count == 0) { return; }

        //遊べない問題を先に並べる
        problems = problems.OrderByDescending(p => p.Blocking).ToList();
        foreach (var p in problems) { Debug.LogWarning("ステージの点検: " + p.Message, p.Target); }
        var lines = problems.Take(MaxLines).Select(p => "・" + p.Message).ToList();
        if (problems.Count > MaxLines) { lines.Add("ほか " + (problems.Count - MaxLines) + " 件（Console に全部出ています）"); }
        var list = string.Join("\n", lines);

        if (problems.Any(p => p.Blocking))
        {
            EditorUtility.DisplayDialog("ステージの点検", "このままでは遊べないので、再生を止めました。\n直してから、もう一度再生してください。\n\n" + list, "OK");
        }
        else if (EditorUtility.DisplayDialog("ステージの点検", "ステージに気になるところがあります。\n\n" + list, "このまま遊ぶ", "やめる"))
        {
            return;
        }

        //再生を止めて、問題のある物を選んで見せる
        EditorApplication.isPlaying = false;
        var targets = problems.Where(p => p.Target != null).Select(p => (Object)p.Target).Distinct().ToArray();
        if (targets.Length > 0)
        {
            Selection.objects = targets;
            EditorGUIUtility.PingObject(targets[0]);
        }
    }

    //開いているシーンを点検して、問題の一覧を返す（問題がなければ空）
    public static List<Problem> Check()
    {
        var problems = new List<Problem>();
        Physics.SyncTransforms(); //動かした直後は当たり判定の位置が古いままなので、そろえてから調べる
        var player = GameObject.FindGameObjectWithTag("Player");
        var dots = GameObject.FindGameObjectsWithTag("Dot");
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Bounds? maze = MazeBounds();

        if (player == null) { problems.Add(new Problem("プレイヤーがいません。消してしまったときは、元に戻してください（Ctrl + Z）。", null)); }
        if (dots.Length == 0) { problems.Add(new Problem("星が 1 つもありません。このままだと、始めた瞬間にクリアになります。", null)); }

        Vector3? start = player != null ? NavPoint(player.transform.position, 1f) : null;
        foreach (var dot in dots)
        {
            var pos = dot.transform.position;
            pos.y = 0f; //星は浮かせてあるので、行けるかどうかは真下の床で調べる
            if (!Inside(maze, pos))
            {
                problems.Add(new Problem("星「" + dot.name + "」が迷路の外にあります。", dot));
            }
            else if (start.HasValue && !Reachable(start.Value, pos, DotReach))
            {
                problems.Add(new Problem("星「" + dot.name + "」は、プレイヤーが取りに行けない場所（壁の中など）にあります。このままだとクリアできません。", dot));
            }
        }
        foreach (var enemy in enemies)
        {
            var pos = enemy.transform.position;
            if (!Inside(maze, pos))
            {
                problems.Add(new Problem("敵「" + enemy.name + "」が迷路の外にいます。", enemy));
            }
            else if (!NavPoint(pos, 0.5f).HasValue)
            {
                problems.Add(new Problem("敵「" + enemy.name + "」が、歩けない場所（壁の中など）にいます。このままだと動けません。", enemy));
            }
            else if (player != null && FlatDistance(pos, player.transform.position) < NearStartDistance)
            {
                problems.Add(new Problem("敵「" + enemy.name + "」が、プレイヤーのスタート地点に近すぎます。始めてすぐ捕まります。", enemy, false));
            }
        }
        return problems;
    }

    //迷路の範囲（Map の中の、床を除いた壁をすべて囲む箱）。迷路が無ければ null
    static Bounds? MazeBounds()
    {
        var map = GameObject.Find(MapName);
        if (map == null) { return null; }
        Bounds? b = null;
        foreach (var col in map.GetComponentsInChildren<Collider>())
        {
            if (col.bounds.size.y < 0.01f) { continue; } //平らな床は除く
            if (b.HasValue) { var e = b.Value; e.Encapsulate(col.bounds); b = e; }
            else { b = col.bounds; }
        }
        return b;
    }

    static bool Inside(Bounds? maze, Vector3 pos)
    {
        if (!maze.HasValue) { return true; }
        var b = maze.Value;
        return pos.x > b.min.x && pos.x < b.max.x && pos.z > b.min.z && pos.z < b.max.z;
    }

    //pos の近く（radius 以内）で、歩ける範囲（NavMesh）の上の点。無ければ null
    static Vector3? NavPoint(Vector3 pos, float radius)
    {
        if (NavMesh.SamplePosition(pos, out var hit, radius, NavMesh.AllAreas)) { return hit.position; }
        return null;
    }

    //start から歩いて、target の reach 以内まで行けるか
    static bool Reachable(Vector3 start, Vector3 target, float reach)
    {
        var end = NavPoint(target, reach);
        if (!end.HasValue) { return false; }
        var path = new NavMeshPath();
        return NavMesh.CalculatePath(start, end.Value, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
