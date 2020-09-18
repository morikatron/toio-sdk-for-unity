using System.Collections.Generic;
using UnityEngine;

namespace toio.tutorial
{
    public class SoundScene : MonoBehaviour
    {
        float intervalTime = 6.0f;
        float elapsedTime = 0;
        Cube cube;
        bool started = false;

        // Start is called before the first frame update
        async void Start()
        {
            var peripheral = await new NearestScanner().Scan();
            cube = await new CubeConnecter().Connect(peripheral);
            // 最初にプリセットの音源を再生
            cube.PlayPresetSound(0);
            started = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!started) return;

            elapsedTime += Time.deltaTime;

            if (intervalTime < elapsedTime) // 6秒ごとに実行
            {
                elapsedTime = 0.0f;

                // カエルの歌の楽譜を作成
                List<Cube.SoundOperation> sound = new List<Cube.SoundOperation>();
                // 継続時間(ミリ秒), 音量(0~255), 音符(0~128)
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.C6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.NO_SOUND));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.C6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.NO_SOUND));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.C6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.NO_SOUND));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.C6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.NO_SOUND));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.C6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.D6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.E6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.F6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.E6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.D6));
                sound.Add(new Cube.SoundOperation(durationMs:300, volume:15, note_number:Cube.NOTE_NUMBER.C6));
                // 楽譜を再生
                cube.PlaySound(1, sound.ToArray());
            }
        }
    }
}