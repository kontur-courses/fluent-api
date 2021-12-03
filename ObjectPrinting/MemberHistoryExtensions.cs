namespace ObjectPrinting
{
    internal static class MemberHistoryExtensions
    {
        internal static bool TryFindMember(this MemberHistory memberHistory, object obj)
        {
            var currentNode = memberHistory;
            while (currentNode.PreviousMemberHistory is not null)
            {
                if (ReferenceEquals(obj, currentNode.Data))
                    return true;
                currentNode = currentNode.PreviousMemberHistory;
            }

            return ReferenceEquals(obj, currentNode.Data);
        }
    }
}