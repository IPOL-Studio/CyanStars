#nullable enable

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.GamePlay.ChartEditor.View;
using UnityEngine;

namespace CyanStars.GamePlay.ChartEditor.Model
{
    /// <summary>
    /// 谱面编辑器 Model 层
    /// </summary>
    public class ChartEditorModel
    {
        // --- 在编辑器内初始化和临时存储的、经过校验后的数据，不会持久化 ---
        /// <summary>
        /// 编辑器精简模式
        /// </summary>
        /// <remarks>精简无关元素方便新手理解制谱器，只允许设置 1 个 BPM 组、曲目版本，隐藏变速和事件相关设置</remarks>
        public bool IsSimplification { get; private set; } = true;

        /// <summary>
        /// 当前选中的画笔（或者是橡皮？）
        /// </summary>
        public EditTool SelectedEditTool { get; private set; } = EditTool.Select;

        /// <summary>
        /// 当前选中编辑的 BPM 组下标
        /// </summary>
        public int? SelectedBpmItemIndex { get; private set; } = null;


        /// <summary>
        /// 当前设置的位置精度（等于屏幕上有几条竖直位置线）
        /// </summary>
        public int PosAccuracy { get; private set; } = 4;

        /// <summary>
        /// 是否开启了位置吸附
        /// </summary>
        /// <remarks>将会吸附到最近的位置线，或者是两条位置线的中点</remarks>
        public bool PosMagnetState { get; private set; } = true;

        /// <summary>
        /// 节拍精度（等于将每小节均分为几分）
        /// </summary>
        public int BeatAccuracy { get; private set; } = 2;

        /// <summary>
        /// 节拍缩放
        /// </summary>
        /// <remarks>编辑器纵向拉伸比例</remarks>
        public float BeatZoom { get; private set; } = 1f;

        /// <summary>
        /// 创建 Hold 音符时暂存的开始拍
        /// </summary>
        /// <remarks>选中 Hold 画笔后第一次点击空白区域</remarks>
        public Beat? TempHoldJudgeBeat { get; private set; } = null;


        /// <summary>
        /// 用于 M 层的音符数据，在 Model 构造时指向 ChartData 中的对象
        /// </summary>
        /// <remarks>NoteView 在从对象池取回时会存储对应的单个音符数据，可以借此定位回来</remarks>
        public HashSet<BaseChartNoteData> ChartNotes { get; private set; } = new HashSet<BaseChartNoteData>();

        /// <summary>
        /// 当前选中的 Note，用 HashSet 是考虑兼容后续框选多个 Note 一起修改
        /// </summary>
        public HashSet<BaseChartNoteData> SelectedNotes { get; private set; } = new HashSet<BaseChartNoteData>();

        /// <summary>
        /// 计算 offset 后，当前选中的音乐的实际时长（ms）
        /// </summary>
        /// <remarks>超过这个时长的内容都不可以编辑，包括音符编辑、事件等等</remarks>
        public int ActualMusicTime { get; set; } // TODO：修改为根据音乐长度自动确定

        /// <summary>
        /// 曲绘材质
        /// </summary>
        public Texture2D? CoverTexture { get; private set; } = null;


        /// <summary>
        /// 谱包信息弹窗可见性
        /// </summary>
        public bool ChartPackDataCanvasVisibleness { get; private set; } = false;

        /// <summary>
        /// 谱面信息弹窗可见性
        /// </summary>
        public bool ChartDataCanvasVisibleness { get; private set; } = false;

        /// <summary>
        /// 音乐版本弹窗可见性
        /// </summary>
        public bool MusicVersionCanvasVisibleness { get; private set; } = false;

        /// <summary>
        /// BPM 组弹窗可见性
        /// </summary>
        public bool BpmGroupCanvasVisibleness { get; private set; } = false;


        // --- 从磁盘加载到内存中的、经过校验后的谱包和谱面等数据，加载/保存时需要从读写磁盘。 ---
        public readonly string WorkspacePath;
        public readonly ChartPackData ChartPackData;
        public readonly ChartData ChartData;
        public List<MusicVersionData> MusicVersionDatas => ChartPackData.MusicVersionDatas;
        public List<BpmGroupItem> BpmGroupDatas => ChartPackData.BpmGroup.Data;
        public List<SpeedGroupData> SpeedGroupDatas => ChartData.SpeedGroupDatas;


        // --- 内部变量 ---
        private string? coverFileName = null;
        private bool needDumpCoverWhenSave = false; // 在保存时需要复制外部的曲绘文件到 Assets 路径下


        #region Model 事件

        /// <summary>
        /// 进入/退出精简模式
        /// </summary>
        public event Action? OnSimplificationChanged;

        /// <summary>
        /// 选中的画笔发生变化
        /// </summary>
        public event Action? OnEditToolChanged;

        /// <summary>
        /// 编辑器属性侧边栏内容变化
        /// </summary>
        public event Action? OnEditorAttributeChanged;

        /// <summary>
        /// 音符属性侧边栏内容变化
        /// </summary>
        public event Action? OnNoteAttributeChanged;

        /// <summary>
        /// 当选中的音符发生了变化（被选中、被取消选中）
        /// </summary>
        public event Action? OnSelectedNotesChanged;

        /// <summary>
        /// 谱包基本信息（名称、预览时间、曲绘）发生变化
        /// </summary>
        public event Action? OnChartPackDataChanged;

        /// <summary>
        /// 谱面基本信息（难度、定数、预备拍数）发生变化
        /// </summary>
        public event Action? OnChartDataChanged;

        /// <summary>
        /// 音乐版本数据发生变化时
        /// </summary>
        public event Action? OnMusicVersionDataChanged;

        /// <summary>
        /// Bpm 组发生变化
        /// </summary>
        public event Action? OnBpmGroupChanged;

        /// <summary>
        /// 选中的 Bpm 组发生变化
        /// </summary>
        public event Action? OnSelectedBpmItemChanged;

        /// <summary>
        /// 变速组发生变化
        /// </summary>
        public event Action? OnSpeedGroupChanged;

        /// <summary>
        /// 音符组发生变化
        /// </summary>
        public event Action? OnNoteDataChanged;

        /// <summary>
        /// 暂存的 Hold 开始拍发生变化
        /// </summary>
        public event Action? OnTempHoldJudgeBeatChanged;

        /// <summary>
        /// 谱包弹窗打开或关闭
        /// </summary>
        public event Action? OnChartPackDataCanvasVisiblenessChanged;

        /// <summary>
        /// 谱面弹窗打开或关闭
        /// </summary>
        public event Action? OnChartDataCanvasVisiblenessChanged;

        /// <summary>
        /// 曲目弹窗打开或关闭
        /// </summary>
        public event Action? OnMusicVersionCanvasVisiblenessChanged;

        /// <summary>
        /// BPM 组弹窗打开或关闭
        /// </summary>
        public event Action? OnBpmGroupCanvasVisiblenessChanged;

        #endregion


        /// <summary>
        /// 构造函数异步工厂方法
        /// </summary>
        /// <param name="workspacePath">谱包工作区绝对路径（谱包索引文件所在的文件夹路径）</param>
        /// <param name="chartPackData">要加载到编辑器的谱包数据</param>
        /// <param name="chartData">要加载到编辑器的谱面数据</param>
        /// <returns>异步返回 EditorModel 实例</returns>
        public static async Task<ChartEditorModel> CreateEditorModel(string workspacePath, ChartPackData chartPackData,
            ChartData chartData)
        {
            ChartEditorModel model = new ChartEditorModel(workspacePath, chartPackData, chartData);
            model.CoverTexture = model.ChartPackData.CoverFilePath != null
                ? await GameRoot.Asset.LoadAssetAsync<Texture2D>(Path.Combine(workspacePath,
                    model.ChartPackData.CoverFilePath))
                : null;
            return model;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workspacePath">谱包工作区绝对路径（谱包索引文件所在的文件夹路径）</param>
        /// <param name="chartPackData">要加载到编辑器的谱包数据</param>
        /// <param name="chartData">要加载到编辑器的谱面数据</param>
        private ChartEditorModel(string workspacePath, ChartPackData chartPackData, ChartData chartData)
        {
            WorkspacePath = workspacePath;
            ChartPackData = chartPackData;
            ChartData = chartData;

            foreach (var note in ChartData.Notes)
            {
                ChartNotes.Add(note);
            }

            ChartPackDataCanvasVisibleness = false;
            ChartDataCanvasVisibleness = false;
            MusicVersionCanvasVisibleness = false;
            BpmGroupCanvasVisibleness = false;
        }

        #region 编辑器管理

        /// <summary>
        /// 保存谱包和谱面文件到磁盘
        /// </summary>
        public void Save()
        {
            // 将 Texture2D 转为文件并储存
            if (needDumpCoverWhenSave)
            {
                needDumpCoverWhenSave = false;
                if (coverFileName == null || CoverTexture == null)
                {
                    Debug.LogError("文件名或材质为空，请检查");
                    return;
                }

                string assetsFolderPath = Path.Combine(WorkspacePath, "Assets");
                if (!Directory.Exists(assetsFolderPath))
                {
                    Directory.CreateDirectory(Path.Combine(assetsFolderPath));
                }

                string newFilePath = Path.Combine(assetsFolderPath, coverFileName);

                byte[] coverBytes;
                switch (Path.GetExtension(coverFileName).ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        coverBytes = CoverTexture.EncodeToJPG(100);
                        break;
                    case ".png":
                        coverBytes = CoverTexture.EncodeToPNG();
                        break;
                    default:
                        Debug.LogError($"不支持的曲绘文件格式：{coverFileName}");
                        coverBytes = new byte[]{};
                        break;
                }

                File.WriteAllBytes(newFilePath, coverBytes);
            }
        }

        public void SetEditTool(EditTool editTool)
        {
            if (SelectedEditTool == editTool)
            {
                return;
            }

            SelectedEditTool = editTool;
            OnEditToolChanged?.Invoke();
        }

        public void SetPosAccuracy(string posAccuracyStr)
        {
            if (!int.TryParse(posAccuracyStr, out int posAccuracy) ||
                posAccuracy < 0)
            {
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (PosAccuracy == posAccuracy)
            {
                return;
            }

            PosAccuracy = posAccuracy;
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetPosMagnetState(bool isOn)
        {
            if (PosMagnetState == isOn)
            {
                return;
            }

            PosMagnetState = isOn;
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetBeatAccuracy(string beatAccuracyStr)
        {
            if (!int.TryParse(beatAccuracyStr, out int beatAccuracy) ||
                beatAccuracy <= 0)
            {
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (BeatAccuracy == beatAccuracy)
            {
                return;
            }

            BeatAccuracy = beatAccuracy;
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetBeatZoom(string beatZoomStr)
        {
            if (!float.TryParse(beatZoomStr, out float beatZoom) ||
                beatZoom <= 0)
            {
                // 不修改值，触发刷新
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (Mathf.Approximately(BeatZoom, beatZoom))
            {
                // 不修改值，不刷新
                return;
            }

            // 赋值，刷新
            BeatZoom = beatZoom;
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetMusicVersionCanvasVisibleness(bool isVisible)
        {
            if (MusicVersionCanvasVisibleness == isVisible)
            {
                return;
            }

            MusicVersionCanvasVisibleness = isVisible;
            OnMusicVersionCanvasVisiblenessChanged?.Invoke();
        }

        public void SetChartPackDataCanvasVisibleness(bool isVisible)
        {
            if (ChartPackDataCanvasVisibleness == isVisible)
            {
                return;
            }

            ChartPackDataCanvasVisibleness = isVisible;
            OnChartPackDataCanvasVisiblenessChanged?.Invoke();
        }

        public void SetChartDataCanvasVisibleness(bool isVisible)
        {
            if (ChartDataCanvasVisibleness == isVisible)
            {
                return;
            }

            ChartDataCanvasVisibleness = isVisible;
            OnChartDataCanvasVisiblenessChanged?.Invoke();
        }

        /// <summary>
        /// 点击某个已存在的音符，由 note 的 button 组件触发
        /// </summary>
        /// <param name="note">音符数据</param>
        public void SelectNote(BaseChartNoteData note)
        {
            // TODO: 拓展兼容选中多个音符
            if (SelectedEditTool == EditTool.Eraser)
            {
                ChartNotes.Remove(note);
                OnNoteDataChanged?.Invoke();
                return;
            }

            SelectedNotes.Clear();
            SelectedNotes.Add(note);
            OnSelectedNotesChanged?.Invoke();
        }

        public void SetBpmGroupCanvasVisibleness(bool isVisible)
        {
            if (BpmGroupCanvasVisibleness == isVisible)
            {
                return;
            }

            BpmGroupCanvasVisibleness = isVisible;
            OnBpmGroupCanvasVisiblenessChanged?.Invoke();
        }

        public void SelectBpmItem(int index)
        {
            if (SelectedBpmItemIndex != index)
            {
                SelectedBpmItemIndex = index;
                OnSelectedBpmItemChanged?.Invoke();
            }
        }

        #endregion

        #region 谱包信息和谱面元数据管理

        /// <summary>
        /// 更新谱包标题
        /// </summary>
        /// <param name="title">新的谱包标题</param>
        public void UpdateChartPackTitle(string title)
        {
            if (ChartPackData.Title != title)
            {
                ChartPackData.Title = title;
                OnChartPackDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 更新预览开始拍
        /// </summary>
        public void UpdatePreviewStareBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!(int.TryParse(integerPartString, out int integerPart) &&
                  int.TryParse(numeratorString, out int numerator) &&
                  int.TryParse(denominatorString, out int denominator)))
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat? newBeat) ||
                ((Beat)newBeat).ToFloat() > ChartPackData.MusicPreviewEndBeat.ToFloat())
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (ChartPackData.MusicPreviewStartBeat != (Beat)newBeat)
            {
                ChartPackData.MusicPreviewStartBeat = (Beat)newBeat;
                OnChartPackDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 更新预览结束拍
        /// </summary>
        public void UpdatePreviewEndBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!(int.TryParse(integerPartString, out int integerPart) &&
                  int.TryParse(numeratorString, out int numerator) &&
                  int.TryParse(denominatorString, out int denominator)))
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat? newBeat) ||
                ((Beat)newBeat).ToFloat() < ChartPackData.MusicPreviewStartBeat.ToFloat())
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (ChartPackData.MusicPreviewEndBeat != (Beat)newBeat)
            {
                ChartPackData.MusicPreviewEndBeat = (Beat)newBeat;
                OnChartPackDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 更新曲绘大图路径，并自动重置裁剪区域（对齐图片中央，尝试裁剪 4:1 最大区域）
        /// </summary>
        /// <param name="path">曲绘大图外部绝对路径</param>
        public async Task UpdateCoverFilePath(string path)
        {
            // 加载 Texture2D 到内存，保存时检查是否需要替换文件，并将 Texture2D 转为文件写入
            needDumpCoverWhenSave = true;
            ChartPackData.CoverFilePath = path;
            coverFileName = Path.GetFileName(path);
            CoverTexture = await GameRoot.Asset.LoadAssetAsync<Texture2D>(path);
            if (CoverTexture == null)
            {
                ChartPackData.CropStartPosition = new Vector2(0, 0);
                ChartPackData.CropHeight = 0;
            }
            else
            {
                const float targetAspectRatio = 4.0f;

                float imageWidth = CoverTexture!.width;
                float imageHeight = CoverTexture!.height;

                // 计算图片的实际宽高比
                float imageAspectRatio = imageWidth / imageHeight;

                // 决定哪个维度是限制因素
                float cropWidth;
                float cropHeight;
                if (imageAspectRatio > targetAspectRatio)
                {
                    // 图片相对“更宽”或“不够高”，高度是限制因素
                    cropHeight = imageHeight;
                    cropWidth = cropHeight * targetAspectRatio;
                }
                else
                {
                    // 图片相对“更高”或“不够宽”，宽度是限制因素
                    cropWidth = imageWidth;
                    cropHeight = cropWidth / targetAspectRatio;
                }

                // 计算裁剪区域的左下角坐标，以使其居中
                float cropX = (imageWidth - cropWidth) / 2.0f;
                float cropY = (imageHeight - cropHeight) / 2.0f;


                ChartPackData.CropStartPosition = new Vector2(cropX, cropY);
                ChartPackData.CropHeight = cropHeight;
            }

            OnChartPackDataChanged?.Invoke();
        }

        /// <summary>
        /// 在裁剪框被拖动时更新裁剪起始位置和高度
        /// </summary>
        /// <remarks>裁剪后的图片必须为横向1:4，且必须在原图范围内</remarks>
        /// <param name="type">裁剪框顶点类型</param>
        /// <param name="pointPositionRatio">当前拖动时的指针位置（相对于原图的宽高比例，已归一化并限制范围在 [0,1]）</param>
        public void UpdateCoverCropByHandles(CoverCropHandleType type, Vector2 pointPositionRatio)
        {
            // 检查数据
            if (CoverTexture == null)
            {
                Debug.LogError("CoverSprite or its texture is not assigned.");
                return;
            }

            float sourceWidth = CoverTexture.width;
            float sourceHeight = CoverTexture.height;
            if (sourceWidth <= 0 || sourceHeight <= 0) return;

            // 将归一化的指针位置转换为像素坐标
            Vector2 currentPointPixels = new Vector2(
                pointPositionRatio.x * sourceWidth,
                pointPositionRatio.y * sourceHeight
            );

            // 获取旧的裁剪框信息（像素单位）
            Vector2 oldBottomLeft = ChartPackData.CropStartPosition;
            float oldCropHeight = ChartPackData.CropHeight;
            float oldCropWidth = oldCropHeight * 4;

            // 确定固定点，即被拖动顶点的对角点，并计算 鼠标-固定点 矩形的宽高
            Vector2 anchorPoint;
            float boundsWidth;
            float boundsHeight;
            switch (type)
            {
                case CoverCropHandleType.TopLeft: // 拖动左上角，则右下角固定
                    anchorPoint = new Vector2(oldBottomLeft.x + oldCropWidth, oldBottomLeft.y);
                    boundsWidth = Mathf.Max(0, anchorPoint.x - currentPointPixels.x);
                    boundsHeight = Mathf.Max(0, currentPointPixels.y - anchorPoint.y);
                    break;
                case CoverCropHandleType.TopRight: // 拖动右上角，则左下角固定
                    anchorPoint = oldBottomLeft;
                    boundsWidth = Mathf.Max(0, currentPointPixels.x - anchorPoint.x);
                    boundsHeight = Mathf.Max(0, currentPointPixels.y - anchorPoint.y);
                    break;
                case CoverCropHandleType.BottomLeft: // 拖动左下角，则右上角固定
                    anchorPoint = new Vector2(oldBottomLeft.x + oldCropWidth, oldBottomLeft.y + oldCropHeight);
                    boundsWidth = Mathf.Max(0, anchorPoint.x - currentPointPixels.x);
                    boundsHeight = Mathf.Max(0, anchorPoint.y - currentPointPixels.y);
                    break;
                case CoverCropHandleType.BottomRight: // 拖动右下角，则左上角固定
                    anchorPoint = new Vector2(oldBottomLeft.x, oldBottomLeft.y + oldCropHeight);
                    boundsWidth = Mathf.Max(0, currentPointPixels.x - anchorPoint.x);
                    boundsHeight = Mathf.Max(0, anchorPoint.y - currentPointPixels.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            // 确定 鼠标-固定点 矩形中的关键因素
            float newCropHeight = boundsWidth / 4.0f >= boundsHeight ? boundsWidth / 4.0f : boundsHeight;

            float maxAllowedHeight;
            switch (type)
            {
                case CoverCropHandleType.TopLeft:
                    maxAllowedHeight = Mathf.Min(anchorPoint.x / 4.0f, sourceHeight - anchorPoint.y);
                    break;
                case CoverCropHandleType.TopRight:
                    maxAllowedHeight = Mathf.Min((sourceWidth - anchorPoint.x) / 4.0f, sourceHeight - anchorPoint.y);
                    break;
                case CoverCropHandleType.BottomLeft:
                    maxAllowedHeight = Mathf.Min(anchorPoint.x / 4.0f, anchorPoint.y);
                    break;
                case CoverCropHandleType.BottomRight:
                    maxAllowedHeight = Mathf.Min((sourceWidth - anchorPoint.x) / 4.0f, anchorPoint.y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            newCropHeight = Mathf.Min(newCropHeight, maxAllowedHeight);

            // 避免尺寸为负或过小
            newCropHeight = Mathf.Max(0, newCropHeight);
            float newCropWidth = newCropHeight * 4.0f;

            // 根据拖动的顶点类型，计算新的裁剪起始位置
            Vector2 newStartPosition = type switch
            {
                CoverCropHandleType.TopLeft =>
                    // 新的左下角 = (右下角.x - 新宽度, 右下角.y)
                    new Vector2(anchorPoint.x - newCropWidth, anchorPoint.y),
                CoverCropHandleType.TopRight =>
                    // 新的左下角 = 左下角 (固定点)
                    anchorPoint,
                CoverCropHandleType.BottomLeft =>
                    // 新的左下角 = (右上角.x - 新宽度, 右上角.y - 新高度)
                    new Vector2(anchorPoint.x - newCropWidth, anchorPoint.y - newCropHeight),
                CoverCropHandleType.BottomRight =>
                    // 新的左下角 = (左上角.x, 左上角.y - 新高度)
                    new Vector2(anchorPoint.x, anchorPoint.y - newCropHeight),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            ChartPackData.CropHeight = newCropHeight;
            ChartPackData.CropStartPosition = newStartPosition;
            OnChartPackDataChanged?.Invoke();
        }

        /// <summary>
        /// 向列表添加一个新的音乐版本
        /// </summary>
        /// <param name="newData">要添加的音乐版本数据</param>
        public void AddMusicVersionItem(MusicVersionData newData = null)
        {
            newData = newData ?? new MusicVersionData();
            foreach (MusicVersionData musicVersionData in MusicVersionDatas)
            {
                if (musicVersionData.AudioFilePath == newData.AudioFilePath)
                {
                    return;
                }
            }

            MusicVersionDatas.Add(newData);

            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 从列表删除音乐版本数据元素
        /// </summary>
        public void DeleteMusicVersionItem(MusicVersionData oldItem)
        {
            MusicVersionDatas.Remove(oldItem);
            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 置顶（设为默认）某个音乐版本
        /// </summary>
        /// <param name="musicVersionItem">音乐版本 item</param>
        public void TopMusicVersionItem(MusicVersionData musicVersionItem)
        {
            MusicVersionDatas.Remove(musicVersionItem);
            MusicVersionDatas.Insert(0, musicVersionItem);
            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 复制某个音乐版本到列表尾
        /// </summary>
        /// <param name="musicVersionItem">音乐版本 item</param>
        public void CopyMusicVersionItem(MusicVersionData musicVersionItem)
        {
            MusicVersionData copiedItem = new MusicVersionData(musicVersionItem.VersionTitle,
                musicVersionItem.AudioFilePath, musicVersionItem.Offset,
                new Dictionary<string, List<string>>());
            foreach (KeyValuePair<string, List<string>> staff in musicVersionItem.Staffs)
            {
                List<string> copiedStaffJobs = new List<string>(staff.Value);
                copiedItem.Staffs.Add(staff.Key, copiedStaffJobs);
            }

            MusicVersionDatas.Add(copiedItem);
            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 更新某个音乐版本的标题
        /// </summary>
        /// <param name="musicVersionData">音乐版本 item</param>
        /// <param name="newTitle">新的标题</param>
        public void UpdateMusicVersionTitle(MusicVersionData musicVersionData, string newTitle)
        {
            int itemIndex = MusicVersionDatas.IndexOf(musicVersionData);
            if (MusicVersionDatas[itemIndex].VersionTitle != newTitle)
            {
                MusicVersionDatas[itemIndex].VersionTitle = newTitle;
                OnMusicVersionDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 校验并更新某个音乐版本的 offset
        /// </summary>
        /// <param name="musicVersionData">音乐版本 item</param>
        /// <param name="newOffsetString">新的 offset</param>
        public void UpdateMusicVersionOffset(MusicVersionData musicVersionData, string newOffsetString)
        {
            if (!int.TryParse(newOffsetString, out int newOffset))
            {
                OnMusicVersionDataChanged?.Invoke();
            }

            int itemIndex = MusicVersionDatas.IndexOf(musicVersionData);
            if (MusicVersionDatas[itemIndex].Offset != newOffset)
            {
                MusicVersionDatas[itemIndex].Offset = newOffset;
                OnMusicVersionDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 增加或减少音乐版本的 offset 指定时长
        /// </summary>
        /// <param name="musicVersionData">音乐版本 item</param>
        /// <param name="addNumber">要增加的时长，ms（为负数时视为减少）</param>
        public void AddMusicVersionOffsetValue(MusicVersionData musicVersionData, int addNumber)
        {
            if (addNumber == 0)
            {
                return;
            }

            int itemIndex = MusicVersionDatas.IndexOf(musicVersionData);
            MusicVersionDatas[itemIndex].Offset += addNumber;
            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 为指定音乐版本添加一行 Staff
        /// </summary>
        /// <param name="musicVersionData">要操作的音乐版本</param>
        public void AddStaffItem(MusicVersionData musicVersionData)
        {
            int i = 1;
            while (musicVersionData.Staffs.ContainsKey("Staff" + i.ToString()))
            {
                i++;
            }

            musicVersionData.Staffs.Add("Staff" + i.ToString(), new List<string>());
            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 为指定音乐版本删除一行 Staff
        /// </summary>
        /// <param name="musicVersionData">要操作的音乐版本</param>
        /// <param name="staffItem">要删除的 Staff</param>
        public void DeleteStaffItem(MusicVersionData musicVersionData, KeyValuePair<string, List<string>> staffItem)
        {
            musicVersionData.Staffs.Remove(staffItem.Key);
            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 更新指定音乐版本中的指定 Staff 信息
        /// </summary>
        /// <param name="musicVersionItem">要操作的音乐版本</param>
        /// <param name="oldStaffItem">要更新的 Staff Item</param>
        /// <param name="newName">新的 Staff 名字</param>
        /// <param name="newJobString">新的 Staff 职务，多个职务斜杠分隔</param>
        public void UpdateStaffItem(MusicVersionData musicVersionItem,
            KeyValuePair<string, List<string>> oldStaffItem, string newName, string newJobString)
        {
            if (oldStaffItem.Key == newName && string.Join("/", oldStaffItem.Value) == newJobString ||
                oldStaffItem.Key != newName && musicVersionItem.Staffs.ContainsKey(newName))
            {
                OnMusicVersionDataChanged?.Invoke();
                return;
            }

            List<string> newJob = new List<string>(newJobString.Split('/'));

            if (oldStaffItem.Key != newName)
            {
                musicVersionItem.Staffs.Remove(oldStaffItem.Key);
                musicVersionItem.Staffs.Add(newName, newJob);
                OnMusicVersionDataChanged?.Invoke();
                return;
            }
            else
            {
                musicVersionItem.Staffs[newName] = newJob;
                OnMusicVersionDataChanged?.Invoke();
                return;
            }
        }

        #endregion

        #region 谱面管理

        public void UpdateDifficulty(ChartDifficulty? difficulty)
        {
            // TODO
            throw new NotSupportedException();
        }

        public void UpdateLevel(string text)
        {
            // TODO
            throw new NotSupportedException();
        }

        public void UpdateReadyBeat(string text)
        {
            if (!int.TryParse(text, out int value))
            {
                OnChartDataChanged?.Invoke();
            }

            if (value < 0)
            {
                OnChartDataChanged?.Invoke();
            }

            if (value != ChartData.ReadyBeat)
            {
                ChartData.ReadyBeat = value;
                OnChartDataChanged?.Invoke();
            }
        }

        #region BPM 组管理

        /// <summary>
        /// 更新 BPM 元素开始时间，并将其自动放在合适的位置
        /// </summary>
        /// <remarks>将根据 beat 自动插入或更新 Bpm 组</remarks>
        public void UpdateBpmGroupItemBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (SelectedBpmItemIndex == null)
            {
                Debug.LogError("未选中 bpm 元素，无法修改开始拍！");
                OnBpmGroupChanged?.Invoke();
                return;
            }

            if (!(int.TryParse(integerPartString, out int integerPart) &&
                  int.TryParse(numeratorString, out int numerator) &&
                  int.TryParse(denominatorString, out int denominator)))
            {
                OnBpmGroupChanged?.Invoke();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat? beat))
            {
                OnBpmGroupChanged?.Invoke();
                return;
            }

            if ((Beat)beat! == BpmGroupDatas[(int)SelectedBpmItemIndex].StartBeat)
            {
                OnBpmGroupChanged?.Invoke();
                return;
            }

            for (int i = 0; i < BpmGroupDatas.Count; i++)
            {
                if (i == SelectedBpmItemIndex)
                {
                    continue;
                }

                BpmGroupItem item = BpmGroupDatas[i];
                if (item.StartBeat == (Beat)beat)
                {
                    OnBpmGroupChanged?.Invoke();
                    return;
                }
            }

            BpmGroupItem itemToUpdate = BpmGroupDatas[(int)SelectedBpmItemIndex];
            itemToUpdate.StartBeat = (Beat)beat;
            BpmGroupDatas.Sort((item1, item2) => item1.StartBeat.ToFloat().CompareTo(item2.StartBeat.ToFloat()));
            SelectedBpmItemIndex = BpmGroupDatas.IndexOf(itemToUpdate);

            OnBpmGroupChanged?.Invoke();
        }

        public void DeleteBpmGroupItem()
        {
            if (SelectedBpmItemIndex == null)
            {
                Debug.LogWarning("未选中 bpm 元素，无法移除。");
                return;
            }

            BpmGroupDatas.RemoveAt((int)SelectedBpmItemIndex);
            SelectedBpmItemIndex = (int)SelectedBpmItemIndex == 0
                ? (int?)null // 如果移除了所有元素，则设为 null
                : Math.Min(BpmGroupDatas.Count - 1, (int)SelectedBpmItemIndex); // 尝试让 index 在不越界的情况下不变
            OnBpmGroupChanged?.Invoke();
        }

        public void AddBpmGroupItem()
        {
            float bpm = BpmGroupDatas[BpmGroupDatas.Count - 1].Bpm;
            Beat beat = BpmGroupDatas[BpmGroupDatas.Count - 1].StartBeat;
            Beat newBeat = new Beat(beat.IntegerPart + 1, beat.Numerator, beat.Denominator);
            BpmGroupItem item = new BpmGroupItem(bpm, newBeat);
            BpmGroupDatas.Add(item);
            SelectedBpmItemIndex = BpmGroupDatas.Count - 1;
            OnBpmGroupChanged?.Invoke();
        }

        #endregion

        #region 变速组管理

        /// <summary>
        /// 在变速组列表末尾添加变速组
        /// </summary>
        /// <param name="speedGroupData">指定的变速组，为空时按照默认值添加</param>
        /// <returns>添加的变速组</returns>
        public SpeedGroupData AddSpeedGroupData(SpeedGroupData speedGroupData = null)
        {
            speedGroupData ??= new SpeedGroupData(type: SpeedGroupType.Relative);

            SpeedGroupDatas.Add(speedGroupData);

            OnSpeedGroupChanged?.Invoke();
            return speedGroupData;
        }

        /// <summary>
        /// 删除并返回指定下标的变速组，下标越界会抛出异常
        /// </summary>
        /// <param name="index">要删除的变速组下标</param>
        /// <returns>弹出的变速组</returns>
        public SpeedGroupData PopSpeedGroupData(int index)
        {
            SpeedGroupData speedGroupData = SpeedGroupDatas[index];
            SpeedGroupDatas.RemoveAt(index);

            OnSpeedGroupChanged?.Invoke();

            return speedGroupData;
        }

        /// <summary>
        /// 用指定的变速组替换某个已有的变速组
        /// </summary>
        /// <param name="speedGroupData">新的变速组</param>
        /// <param name="index">下标</param>
        public void UpdateSpeedGroupData(SpeedGroupData speedGroupData, int index)
        {
            SpeedGroupDatas[index] = speedGroupData;

            OnSpeedGroupChanged?.Invoke();
        }

        #endregion

        #region 音符组管理

        /// <summary>
        /// 点击 Content 空白处以创建音符
        /// </summary>
        public void CreateNote(float pos, Beat beat)
        {
            if (SelectedNotes.Count != 0)
            {
                SelectedNotes.Clear();
                OnSelectedNotesChanged?.Invoke();
            }

            if (SelectedEditTool == EditTool.HoldPen)
            {
                if (TempHoldJudgeBeat == null)
                {
                    // 点击的是开始位置，暂存 beat
                    TempHoldJudgeBeat = beat;
                    OnTempHoldJudgeBeatChanged?.Invoke();
                    return;
                }
                else if (((Beat)TempHoldJudgeBeat).ToFloat() < beat.ToFloat())
                {
                    // 点击的是结束位置，创建 HoldNote
                    ChartNotes.Add(new HoldChartNoteData(pos, (Beat)TempHoldJudgeBeat, beat));
                    OnNoteDataChanged?.Invoke();
                    TempHoldJudgeBeat = null;
                    OnTempHoldJudgeBeatChanged?.Invoke();
                    return;
                }
                else
                {
                    // 结束位置小于等于开始位置，放弃创建
                    TempHoldJudgeBeat = null;
                    OnTempHoldJudgeBeatChanged?.Invoke();
                    Debug.LogWarning("EditorModel: 无法创建持续时长小于等于 0 的 HoldNote。");
                    return;
                }
            }

            if (TempHoldJudgeBeat != null)
            {
                // 使用其他画笔点击时，清除暂存的 Hold 开始拍
                TempHoldJudgeBeat = null;
                OnTempHoldJudgeBeatChanged?.Invoke();
            }

            switch (SelectedEditTool)
            {
                // HoldNote 的创建逻辑在上文处理
                case EditTool.Select:
                case EditTool.Eraser:
                    return;
                case EditTool.TapPen:
                    if (pos < 0 || 0.8f < pos)
                    {
                        return;
                    }

                    ChartNotes.Add(new TapChartNoteData(pos, beat));
                    OnNoteDataChanged?.Invoke();
                    return;
                case EditTool.DragPen:
                    if (pos < 0 || 0.8f < pos)
                    {
                        return;
                    }

                    ChartNotes.Add(new DragChartNoteData(pos, beat));
                    OnNoteDataChanged?.Invoke();
                    return;
                case EditTool.ClickPen:
                    if (pos < 0 || 0.8f < pos)
                    {
                        return;
                    }

                    ChartNotes.Add(new ClickChartNoteData(pos, beat));
                    OnNoteDataChanged?.Invoke();
                    return;
                case EditTool.BreakPen:
                    if (Mathf.Approximately(pos, -1))
                    {
                        ChartNotes.Add(new BreakChartNoteData(BreakNotePos.Left, beat));
                        OnNoteDataChanged?.Invoke();
                        return;
                    }
                    else if (Mathf.Approximately(pos, 2))
                    {
                        ChartNotes.Add(new BreakChartNoteData(BreakNotePos.Right, beat));
                        OnNoteDataChanged?.Invoke();
                        return;
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 为选中的 Note 设置判定拍。null 代表不修改此字段而保留原值，以兼容框选多个 note 统一修改。
        /// </summary>
        public void SetJudgeBeat(string integerPart, string numerator, string denominator)
        {
            int? b1 = (int.TryParse(integerPart, out int n1)) ? (int?)n1 : null; // 整数
            int? b2 = (int.TryParse(numerator, out int n2)) ? (int?)n2 : null; // 分子
            int? b3 = (int.TryParse(denominator, out int n3)) ? (int?)n3 : null; // 分母

            if (b1 == null && b2 == null && b3 == null)
            {
                // 输入无有效数字，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            SetNotesJudgeBeat(b1, b2, b3);
        }

        private void SetNotesJudgeBeat(int? integerPart = null, int? numerator = null, int? denominator = null)
        {
            if (integerPart is null && numerator is null && denominator is null)
            {
                return;
            }

            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (!Beat.TryCreateBeat(integerPart ?? note.JudgeBeat.IntegerPart,
                        numerator ?? note.JudgeBeat.Numerator,
                        denominator ?? note.JudgeBeat.Denominator,
                        out Beat? beat))
                {
                    // 合法性校验失败，通知刷新
                    OnNoteAttributeChanged?.Invoke();
                    continue;
                }

                Beat newJudgeBeat = (Beat)beat;

                if (note.JudgeBeat == newJudgeBeat)
                {
                    continue;
                }

                note.JudgeBeat = newJudgeBeat;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        /// <summary>
        /// 为选中的 Note 设置结束拍，非 HoldNote 将不发生变化。null 代表不修改此字段而保留原值，以兼容框选多个 note 统一修改。
        /// </summary>
        public void SetEndBeat(string integerPart, string numerator, string denominator)
        {
            int? b1 = (int.TryParse(integerPart, out int n1)) ? (int?)n1 : null; // 整数
            int? b2 = (int.TryParse(numerator, out int n2)) ? (int?)n2 : null; // 分子
            int? b3 = (int.TryParse(denominator, out int n3)) ? (int?)n3 : null; // 分母

            if (b1 == null && b2 == null && b3 == null)
            {
                // 输入无有效数字，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            SetNotesEndBeat(b1, b2, b3);
        }

        private void SetNotesEndBeat(int? integerPart = null, int? numerator = null, int? denominator = null)
        {
            if (integerPart is null && numerator is null && denominator is null)
            {
                return;
            }

            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (note.Type != NoteType.Hold)
                {
                    Debug.Log($"EditorModel: 此 Note 不是 HoldNote，将不设定 EndBeat。{note} ");
                    continue;
                }

                HoldChartNoteData holdNote = (HoldChartNoteData)note;

                if (!Beat.TryCreateBeat(integerPart ?? holdNote.EndJudgeBeat.IntegerPart,
                        numerator ?? holdNote.EndJudgeBeat.Numerator,
                        denominator ?? holdNote.EndJudgeBeat.Denominator,
                        out Beat? beat))
                {
                    // 合法性校验失败，通知刷新
                    OnNoteAttributeChanged?.Invoke();
                    continue;
                }

                Beat newEndBeat = (Beat)beat;

                if (holdNote.EndJudgeBeat == newEndBeat)
                {
                    continue;
                }

                holdNote.EndJudgeBeat = newEndBeat;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        /// <summary>
        /// 为选中的 Note 设置位置，BreakNote 将不发生变化。
        /// </summary>
        public void SetPos(string posStr)
        {
            if (!float.TryParse(posStr, out float pos))
            {
                // 输入无法转换为 float，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            if (pos < 0 || pos > 0.8f)
            {
                // pos 不在有效范围，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (note.Type == NoteType.Break)
                {
                    Debug.Log($"EditorModel: 此 Note 是 BreakNote，将不设定 Pos。{note}");
                    continue;
                }

                IChartNoteNormalPos posNote = (IChartNoteNormalPos)note;

                if (Mathf.Approximately(posNote.Pos, pos))
                {
                    continue;
                }

                posNote.Pos = pos;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        public void SetBreakPos(BreakNotePos breakPos)
        {
            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (note.Type != NoteType.Break)
                {
                    Debug.Log($"EditorModel: 此 Note 不是 BreakNote，将不设定 BreakPos。{note}");
                    continue;
                }

                BreakChartNoteData breakNote = (BreakChartNoteData)note;

                if (breakNote.BreakNotePos == breakPos)
                {
                    continue;
                }

                breakNote.BreakNotePos = breakPos;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        #endregion

        // TODO: ChartTrackData 轨道拓展数据下个版本再说

        #endregion
    }
}
