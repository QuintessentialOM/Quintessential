using System;

public class UnlockReqCompleteMany(string[] requiredItemIds) : UnlockRequirement {
    public readonly string[] requiredItemIds = requiredItemIds;
}
public class UnlockReqCompleteSomeOfMany(string[] requiredItemIds, int required) : UnlockRequirement {
    public readonly string[] requiredItemIds = requiredItemIds;
    public readonly int required = required;
}
public class UnlockReqCustom(Func<CampaignChapter, CampaignItem, bool> check) : UnlockRequirement {
    public readonly Func<CampaignChapter, CampaignItem, bool> check = check;
}
public class UnlockReqNever() : UnlockRequirement {

}

