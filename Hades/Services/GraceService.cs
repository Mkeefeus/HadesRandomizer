using EldenRingParamsEditor;

namespace Hades.Services;

public static class GraceService
{
    public static void SetGraceEventFlagIds(ParamsEditor editor, IReadOnlyDictionary<uint, int[]> eventFlagGraceDict)
    {
        foreach (var (eventFlagId, graceIds) in eventFlagGraceDict)
        {
            foreach (int graceId in graceIds)
            {
                editor.SetGraceEventFlagId(graceId, eventFlagId);
            }
        }
    }
}