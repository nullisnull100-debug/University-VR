using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Attendance and grading system for instructors.
/// Tracks student attendance, manages grade assignment, and exports data.
/// </summary>
public class AttendanceGradeManager : MonoBehaviour
{
    [System.Serializable]
    public class StudentRecord
    {
        public string studentId;
        public string studentName;
        public string email;
        public List<AttendanceEntry> attendanceHistory = new List<AttendanceEntry>();
        public List<GradeEntry> grades = new List<GradeEntry>();
        public float totalClassTime;
        public int sessionsAttended;
    }

    [System.Serializable]
    public class AttendanceEntry
    {
        public System.DateTime date;
        public bool present;
        public float duration; // minutes
        public string notes;
    }

    [System.Serializable]
    public class GradeEntry
    {
        public string assignmentName;
        public string grade;
        public float score;
        public float maxScore;
        public System.DateTime dateAssigned;
        public string feedback;
    }

    [Header("UI References")]
    public GameObject attendancePanel;
    public GameObject gradesPanel;
    public Transform studentRecordContainer;
    public GameObject studentRecordPrefab;
    
    [Header("Attendance UI")]
    public Text sessionDateText;
    public Button markPresentButton;
    public Button markAbsentButton;
    public Button autoAttendanceButton;
    public InputField attendanceNotesInput;
    
    [Header("Grades UI")]
    public InputField assignmentNameInput;
    public InputField scoreInput;
    public InputField maxScoreInput;
    public InputField feedbackInput;
    public Dropdown letterGradeDropdown;
    public Button assignGradeButton;

    [Header("Export")]
    public Button exportCsvButton;
    public Button exportPdfButton;
    public Text exportStatusText;

    [Header("Settings")]
    public string[] letterGrades = { "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "F" };
    public float[] gradeThresholds = { 93, 90, 87, 83, 80, 77, 73, 70, 67, 60, 0 };
    public float lateArrivalThreshold = 10f; // minutes
    public float minAttendanceTime = 30f; // minutes to count as present

    private Dictionary<string, StudentRecord> studentRecords = new Dictionary<string, StudentRecord>();
    private List<string> selectedStudentIds = new List<string>();
    private string currentSessionId;

    public static AttendanceGradeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSavedRecords();
    }

    void Start()
    {
        SetupUI();
        StartNewSession();
    }

    void SetupUI()
    {
        // Letter grade dropdown
        if (letterGradeDropdown != null)
        {
            letterGradeDropdown.ClearOptions();
            letterGradeDropdown.AddOptions(new List<string>(letterGrades));
        }

        // Button listeners
        if (markPresentButton != null)
            markPresentButton.onClick.AddListener(() => MarkSelectedAttendance(true));
        
        if (markAbsentButton != null)
            markAbsentButton.onClick.AddListener(() => MarkSelectedAttendance(false));
        
        if (autoAttendanceButton != null)
            autoAttendanceButton.onClick.AddListener(AutoMarkAttendance);
        
        if (assignGradeButton != null)
            assignGradeButton.onClick.AddListener(AssignGradeToSelected);
        
        if (exportCsvButton != null)
            exportCsvButton.onClick.AddListener(ExportToCsv);
        
        if (exportPdfButton != null)
            exportPdfButton.onClick.AddListener(ExportToPdf);
    }

    /// <summary>
    /// Start a new attendance session.
    /// </summary>
    public void StartNewSession()
    {
        currentSessionId = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        if (sessionDateText != null)
        {
            sessionDateText.text = $"Session: {System.DateTime.Now:MMM dd, yyyy h:mm tt}";
        }

        Debug.Log($"Started new session: {currentSessionId}");
    }

    /// <summary>
    /// Add or update a student record.
    /// </summary>
    public void AddStudent(string studentId, string name, string email = "")
    {
        if (!studentRecords.ContainsKey(studentId))
        {
            studentRecords[studentId] = new StudentRecord
            {
                studentId = studentId,
                studentName = name,
                email = email
            };
        }
        else
        {
            studentRecords[studentId].studentName = name;
            if (!string.IsNullOrEmpty(email))
            {
                studentRecords[studentId].email = email;
            }
        }

        RefreshStudentList();
    }

    /// <summary>
    /// Mark attendance for selected students.
    /// </summary>
    public void MarkSelectedAttendance(bool present)
    {
        string notes = attendanceNotesInput?.text ?? "";

        foreach (string studentId in selectedStudentIds)
        {
            MarkAttendance(studentId, present, 0f, notes);
        }

        RefreshStudentList();
    }

    /// <summary>
    /// Mark attendance for a specific student.
    /// </summary>
    public void MarkAttendance(string studentId, bool present, float duration, string notes = "")
    {
        if (!studentRecords.ContainsKey(studentId)) return;

        var record = studentRecords[studentId];
        
        // Check if already marked for today
        var today = System.DateTime.Today;
        var existingEntry = record.attendanceHistory.Find(e => e.date.Date == today);
        
        if (existingEntry != null)
        {
            existingEntry.present = present;
            existingEntry.duration = duration;
            existingEntry.notes = notes;
        }
        else
        {
            record.attendanceHistory.Add(new AttendanceEntry
            {
                date = System.DateTime.Now,
                present = present,
                duration = duration,
                notes = notes
            });
        }

        if (present)
        {
            record.sessionsAttended++;
            record.totalClassTime += duration;
        }

        SaveRecords();
        Debug.Log($"Marked {record.studentName} as {(present ? "present" : "absent")}");
    }

    /// <summary>
    /// Automatically mark attendance based on time in class.
    /// </summary>
    public void AutoMarkAttendance()
    {
        var adminPanel = InstructorAdminPanel.Instance;
        if (adminPanel == null) return;

        // This would integrate with the admin panel's student tracking
        Debug.Log("Auto-marking attendance based on session time...");
        
        // Mark all students who have been in class for minimum time
        // Implementation depends on InstructorAdminPanel integration
    }

    /// <summary>
    /// Assign a grade to selected students.
    /// </summary>
    public void AssignGradeToSelected()
    {
        string assignmentName = assignmentNameInput?.text ?? "Assignment";
        float score = 0f;
        float maxScore = 100f;
        string feedback = feedbackInput?.text ?? "";

        if (scoreInput != null)
        {
            float.TryParse(scoreInput.text, out score);
        }
        
        if (maxScoreInput != null)
        {
            float.TryParse(maxScoreInput.text, out maxScore);
        }

        string letterGrade = CalculateLetterGrade(score, maxScore);

        foreach (string studentId in selectedStudentIds)
        {
            AssignGrade(studentId, assignmentName, letterGrade, score, maxScore, feedback);
        }

        // Clear inputs
        if (assignmentNameInput != null) assignmentNameInput.text = "";
        if (scoreInput != null) scoreInput.text = "";
        if (feedbackInput != null) feedbackInput.text = "";

        RefreshStudentList();
    }

    /// <summary>
    /// Assign a grade to a specific student.
    /// </summary>
    public void AssignGrade(string studentId, string assignmentName, string letterGrade, 
                           float score, float maxScore, string feedback = "")
    {
        if (!studentRecords.ContainsKey(studentId)) return;

        var record = studentRecords[studentId];
        
        // Check if grade already exists for this assignment
        var existingGrade = record.grades.Find(g => g.assignmentName == assignmentName);
        
        if (existingGrade != null)
        {
            existingGrade.grade = letterGrade;
            existingGrade.score = score;
            existingGrade.maxScore = maxScore;
            existingGrade.feedback = feedback;
            existingGrade.dateAssigned = System.DateTime.Now;
        }
        else
        {
            record.grades.Add(new GradeEntry
            {
                assignmentName = assignmentName,
                grade = letterGrade,
                score = score,
                maxScore = maxScore,
                dateAssigned = System.DateTime.Now,
                feedback = feedback
            });
        }

        SaveRecords();
        Debug.Log($"Assigned {letterGrade} ({score}/{maxScore}) to {record.studentName} for {assignmentName}");
    }

    /// <summary>
    /// Calculate letter grade from percentage.
    /// </summary>
    string CalculateLetterGrade(float score, float maxScore)
    {
        float percentage = (score / maxScore) * 100f;
        
        for (int i = 0; i < gradeThresholds.Length; i++)
        {
            if (percentage >= gradeThresholds[i])
            {
                return letterGrades[i];
            }
        }
        
        return "F";
    }

    /// <summary>
    /// Get overall grade for a student.
    /// </summary>
    public string GetOverallGrade(string studentId)
    {
        if (!studentRecords.ContainsKey(studentId)) return "N/A";

        var record = studentRecords[studentId];
        if (record.grades.Count == 0) return "N/A";

        float totalScore = 0f;
        float totalMaxScore = 0f;

        foreach (var grade in record.grades)
        {
            totalScore += grade.score;
            totalMaxScore += grade.maxScore;
        }

        return CalculateLetterGrade(totalScore, totalMaxScore);
    }

    /// <summary>
    /// Get attendance rate for a student.
    /// </summary>
    public float GetAttendanceRate(string studentId)
    {
        if (!studentRecords.ContainsKey(studentId)) return 0f;

        var record = studentRecords[studentId];
        if (record.attendanceHistory.Count == 0) return 0f;

        int presentCount = record.attendanceHistory.FindAll(e => e.present).Count;
        return (float)presentCount / record.attendanceHistory.Count * 100f;
    }

    void RefreshStudentList()
    {
        if (studentRecordContainer == null || studentRecordPrefab == null) return;

        // Clear existing items
        foreach (Transform child in studentRecordContainer)
        {
            Destroy(child.gameObject);
        }

        // Create items for each student
        foreach (var kvp in studentRecords)
        {
            CreateStudentRecordItem(kvp.Value);
        }
    }

    void CreateStudentRecordItem(StudentRecord record)
    {
        GameObject item = Instantiate(studentRecordPrefab, studentRecordContainer);
        
        Text nameText = item.GetComponentInChildren<Text>();
        if (nameText != null)
        {
            float attendanceRate = GetAttendanceRate(record.studentId);
            string overallGrade = GetOverallGrade(record.studentId);
            nameText.text = $"{record.studentName} | Attendance: {attendanceRate:F0}% | Grade: {overallGrade}";
        }

        Toggle toggle = item.GetComponentInChildren<Toggle>();
        if (toggle != null)
        {
            string id = record.studentId;
            toggle.onValueChanged.AddListener((selected) => OnStudentSelected(id, selected));
        }
    }

    void OnStudentSelected(string studentId, bool selected)
    {
        if (selected && !selectedStudentIds.Contains(studentId))
        {
            selectedStudentIds.Add(studentId);
        }
        else if (!selected)
        {
            selectedStudentIds.Remove(studentId);
        }
    }

    /// <summary>
    /// Export records to CSV format.
    /// </summary>
    public void ExportToCsv()
    {
        var sb = new System.Text.StringBuilder();
        
        // Header
        sb.AppendLine("Student ID,Name,Email,Sessions Attended,Total Time (min),Attendance Rate,Overall Grade");
        
        // Data
        foreach (var record in studentRecords.Values)
        {
            float attendanceRate = GetAttendanceRate(record.studentId);
            string overallGrade = GetOverallGrade(record.studentId);
            
            sb.AppendLine($"{record.studentId},{record.studentName},{record.email}," +
                         $"{record.sessionsAttended},{record.totalClassTime:F0}," +
                         $"{attendanceRate:F1}%,{overallGrade}");
        }

        string csv = sb.ToString();
        
        // Copy to clipboard
        GUIUtility.systemCopyBuffer = csv;
        
        if (exportStatusText != null)
        {
            exportStatusText.text = "CSV data copied to clipboard!";
        }
        
        Debug.Log($"Exported CSV:\n{csv}");
    }

    /// <summary>
    /// Export records to PDF format (placeholder).
    /// </summary>
    public void ExportToPdf()
    {
        // In production, this would generate an actual PDF
        // For now, show a message
        if (exportStatusText != null)
        {
            exportStatusText.text = "PDF export requires additional library. Use CSV export for now.";
        }
        
        Debug.Log("PDF export not yet implemented");
    }

    void SaveRecords()
    {
        // In production, save to backend server
        // For demo, save to PlayerPrefs (limited)
        string json = JsonUtility.ToJson(new SerializableRecords { records = new List<StudentRecord>(studentRecords.Values) });
        PlayerPrefs.SetString("StudentRecords", json);
        PlayerPrefs.Save();
    }

    void LoadSavedRecords()
    {
        string json = PlayerPrefs.GetString("StudentRecords", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var data = JsonUtility.FromJson<SerializableRecords>(json);
                foreach (var record in data.records)
                {
                    studentRecords[record.studentId] = record;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load student records: {e.Message}");
            }
        }
    }

    [System.Serializable]
    private class SerializableRecords
    {
        public List<StudentRecord> records;
    }
}
