using SubtitleGenerator.Commons.Extensions;
using SubtitleGenerator.Commons.Sets;
using Whisper;

namespace SubtitleGenerator;

public partial class FMain : Form
{
    public FMain()
    {
        InitializeComponent();
    }

    private void FMain_Load(object sender, EventArgs e)
    {
        try
        {
            CustomInit();
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void BtnSelectInputFile_Click(object sender, EventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new()
            {
                FilterIndex = 1,
                Filter = "視訊檔|*.mp4;*.mkv;*.webm|音訊檔|*.wav;*.m4a;*.webm;*.opus;*.ogg;*.mp3;*.mka;*.flac;",
                Title = "請選擇檔案"
            };

            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                TBInputFilePath.Text = openFileDialog.FileName;
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBLanguages_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            CBEnableTranslate_CheckedChanged(this, EventArgs.Empty);
            CBEnableOpenCCS2TWP_CheckedChanged(this, EventArgs.Empty);
            CBEnableOpenCCTW2SP_CheckedChanged(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBEnableTranslate_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            // 因為英文不需要再翻譯成英文，故加入此限制。
            if (CBLanguages.Text == "en" && CBEnableTranslate.Checked)
            {
                CBEnableTranslate.Checked = false;

                ShowWarnMsg(this, "此選項僅於使用對非 en 語言時可以使用。");
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBEnableOpenCCS2TWP_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            if (CBLanguages.Text != "zh" && CBEnableOpenCCS2TWP.Checked)
            {
                CBEnableOpenCCS2TWP.Checked = false;

                ShowWarnMsg(this, "此選項僅於使用 zh 語言時可以使用。");
            }
            else if (CBEnableOpenCCS2TWP.Checked && CBEnableOpenCCTW2SP.Checked)
            {
                CBEnableOpenCCTW2SP.Checked = false;
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void CBEnableOpenCCTW2SP_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            if (CBLanguages.Text != "zh" && CBEnableOpenCCTW2SP.Checked)
            {
                CBEnableOpenCCTW2SP.Checked = false;

                ShowWarnMsg(this, "此選項僅於使用 zh 語言時可以使用。");
            }
            else if (CBEnableOpenCCTW2SP.Checked && CBEnableOpenCCS2TWP.Checked)
            {
                CBEnableOpenCCS2TWP.Checked = false;
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private async void BtnTranscribe_Click(object sender, EventArgs e)
    {
        Control[] ctrlSet1 =
        {
            TBInputFilePath,
            BtnSelectInputFile,
            CBModels,
            CBLanguages,
            CBEnableTranslate,
            CBSamplingStrategies,
            CBExportWebVTT,
            CBEnableOpenCCS2TWP,
            CBEnableOpenCCTW2SP,
            BtnTranscribe,
            BtnReset
        };

        Control[] ctrlSet2 =
        {
            BtnCancel
        };

        try
        {
            if (string.IsNullOrEmpty(TBInputFilePath.Text))
            {
                MessageBox.Show(
                    "請先選擇視訊或音訊檔案。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ctrlSet1.SetEnabled(false);
            ctrlSet2.SetEnabled(true);

            GlobalCTS = new();
            GlobalCT = GlobalCTS.Token;

            TBLog.Clear();

            PBProgress.Style = ProgressBarStyle.Marquee;

            SetOpenCCVariables();

            await Transcribe(
                inputFilePath: TBInputFilePath.Text,
                language: CBLanguages.Text,
                enableTranslate: CBEnableTranslate.Checked,
                enableSpeedUpAudio: false,
                exportWebVtt: CBExportWebVTT.Checked,
                enableConvertToWav: false,
                modelImplementation: eModelImplementation.GPU,
                gpuModelFlags: eGpuModelFlags.None,
                adapter: null,
                ggmlType: GetModelType(CBModels.Text),
                samplingStrategyType: GetSamplingStrategyType(CBSamplingStrategies.Text),
                cancellationToken: GlobalCT);
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
        finally
        {
            ctrlSet1.SetEnabled(true);
            ctrlSet2.SetEnabled(false);

            PBProgress.Style = ProgressBarStyle.Blocks;
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        try
        {
            if (!GlobalCTS.IsCancellationRequested)
            {
                GlobalCTS.Cancel();
            }
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        try
        {
            // 重設變數。
            EnableOpenCC = false;
            GlobalOCCMode = EnumSet.OpenCCMode.None;

            // 重設控制項。 
            TBInputFilePath.Clear();
            CBModels.Text = "Small";
            CBLanguages.Text = "zh";
            CBSamplingStrategies.Text = "Default";
            CBEnableTranslate.Checked = false;
            CBExportWebVTT.Checked = false;
            CBEnableOpenCCS2TWP.Checked = false;
            CBEnableOpenCCTW2SP.Checked = false;
            TBLog.Clear();
        }
        catch (Exception ex)
        {
            ShowErrMsg(this, ex.ToString());
        }
    }
}