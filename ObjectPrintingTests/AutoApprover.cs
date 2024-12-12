using System.Diagnostics;
using ApprovalTests.Core;

namespace ObjectPrintingTests;

public class AutoApprover : IReporterWithApprovalPower
{
    public static readonly AutoApprover Instance = new();

    private string approved;
    private string received;

    public void Report(string approved, string received)
    {
        this.approved = approved;
        this.received = received;
        Trace.WriteLine($@"Will auto-copy ""{received}"" to ""{approved}""");
    }

    public bool ApprovedWhenReported()
    {
        if (!File.Exists(received)) return false;
        File.Delete(approved);
        if (File.Exists(approved)) return false;
        File.Copy(received, approved);
        if (!File.Exists(approved)) return false;

        return true;
    }
}