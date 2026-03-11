using System;
using Raylib_cs;
using QuickSound;



#region --- VAR ---





#endregion




#region --- MSG ---


Flow.Run(Start, Update, Quit, devName: "Moenen");


static void Start () {

	QWave.SaveWaveFileFromAudioFile(@"D:\Test 0.wav", "");
	QWave.SaveWaveFileFromAudioFile(@"D:\Test 1.wav", "");
	QWave.SaveWaveFileFromAudioFile(@"D:\Test 2.wav", "");

}


static void Update () {





}


static void Quit () {

}


#endregion




#region --- LGC ---





#endregion
