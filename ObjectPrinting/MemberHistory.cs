namespace ObjectPrinting
{
    internal class MemberHistory
    {
        internal readonly object Data;
        internal readonly MemberHistory PreviousMemberHistory;

        internal MemberHistory(object data, MemberHistory previousMemberHistory)
        {
            Data = data;
            PreviousMemberHistory = previousMemberHistory;
        }
    }
}