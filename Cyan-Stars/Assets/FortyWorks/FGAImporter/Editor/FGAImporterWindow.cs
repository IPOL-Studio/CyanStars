using System.IO;
using UnityEditor;
using UnityEngine;

namespace FortyWorks.FGAImporter
{
	public class FGAImporterWindow : EditorWindow
	{
		private static FGAImporterWindow _mainWindow;
		
		[MenuItem("Window/Visual Effects/FGAImporter")]
		public static void Open()
		{
			if (_mainWindow == null)
			{
				_mainWindow = CreateInstance<FGAImporterWindow>();
				_mainWindow.titleContent = new GUIContent("FGA Importer");
			}
			_mainWindow.Show();
		}

		private void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope())
			{
				if (GUILayout.Button("Import FGA Texture"))
				{
					var fgaFileName = EditorUtility.OpenFilePanel("Open .fga file", "", "fga");
					var fileName = EditorUtility.SaveFilePanelInProject("Save As..", Path.GetFileNameWithoutExtension(fgaFileName), "asset", "Save as 3D Texture");
					var fgaContent = File.ReadAllText(fgaFileName);

					if (fgaFileName.Length == 0 || fileName.Length == 0)
						return;
                    
					var baker = new FGAParser(fgaContent, fileName);
					baker.Parse();
				}
			}
		}
    }
}

