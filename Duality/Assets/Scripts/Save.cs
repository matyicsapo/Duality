using UnityEngine;
using System.Runtime.Serialization;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

public class Save {
	/// <summary>
	/// Custom Serializable struct for 2D vectors.
	/// </summary>
	[System.Serializable]
	public struct Vect2Int {
		public int x;
		public int y;
		
		public Vect2Int (Vector2 v) {
			x = (int)v.x;
			y = (int)v.y;
		}
		
		public Vect2Int (Vector3 v) {
			x = (int)v.x;
			y = (int)v.y;
		}
	}
	
	[System.Serializable]
	public struct Level {
		public string levelName;
		public Vect2Int size;
		
		public string desc;
		
		public Vect2Int[] blocks;
		
		public Vect2Int enemyCore;
		public Vect2Int[] enemies;
		
		public Vect2Int playerPos;
		public Vect2Int playerFaceDir;
		public int playerLifeCnt;
	}
	
	public sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder {
	    public override System.Type BindToType (string assemblyName, string typeName) {
	        System.Type typeToDeserialize = null;
	
	        System.String currentAssembly = Assembly.GetExecutingAssembly().FullName;
	
	        // In this case we are always using the current assembly
	        assemblyName = currentAssembly;
	
	        // Get the type using the typeName and assemblyName
	        typeToDeserialize = System.Type.GetType(System.String.Format("{0}, {1}", typeName, assemblyName));
	
	        return typeToDeserialize;
	    }
	}
	
	public static void Serialize (Level level) {
		string fileName = "Levels/" + level.levelName + ".lev";
		
		FileStream fs;
		if (File.Exists(fileName)) {
			fs = new FileStream(fileName, FileMode.Truncate, FileAccess.Write);
		}
		else {
			fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
		}
		
		StreamWriter sw = new StreamWriter(fs);
		BinaryFormatter bf = new BinaryFormatter();
		
		MemoryStream ms = new MemoryStream(); // has to be cleared
		
		bf.Serialize(ms, level);
		string s = System.Convert.ToBase64String(ms.ToArray());
		sw.WriteLine(s);
		
		sw.Close();
		fs.Close();
	}
	
	public static Level DeSerialize (string levelName) {
		string fileName = "Levels/" + levelName + ".lev";
		
		FileStream fs = null;
		fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		
		StreamReader sr = new StreamReader(fs);
		
		Level level = new Level();
		
		if (!sr.EndOfStream) {
			string s = sr.ReadLine();
			byte[] ba = System.Convert.FromBase64String(s);
			
			MemoryStream ms = new MemoryStream(ba);
			BinaryFormatter bf = new BinaryFormatter();
			
			//bf.AssemblyFormat = FormatterAssemblyStyle.Simple;
			bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
			
			level = (Level)bf.Deserialize(ms);
		}
		
		sr.Close();
		fs.Close();
		
		return level;
	}
}
