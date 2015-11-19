Module Module1

    Sub Main()
        Using d_stories As New storiesDataContext

            'Before launching that script, verify that the Stories module is only used on portal 0 in table AP_Stories_Tags

            Console.WriteLine("==== Modifying story tags to attach them to Stories modules instead of Portal")

            Console.WriteLine("== Duplicating existing tags attached to Portal for each Stories module")

            'Loop on Story modules
            For Each storyModule In d_stories.AP_Stories_Modules

                Dim storyModuleId = storyModule.StoryModuleId

                'Loop on Story tags
                For Each storyTag In d_stories.AP_Stories_Tags

                    'Duplicate tag and set StoryModuleId 
                    Dim tagToInsert As New stories.AP_Stories_Tag
                    tagToInsert.TagName = storyTag.TagName
                    tagToInsert.PortalId = storyTag.PortalId
                    tagToInsert.Keywords = storyTag.Keywords
                    tagToInsert.Master = storyTag.Master
                    tagToInsert.StoryModuleId = storyModuleId
                    d_stories.AP_Stories_Tags.InsertOnSubmit(tagToInsert)

                Next

                Console.WriteLine("Duplicated existing tags for story module id '" + storyModuleId + "'")

            Next
            d_stories.SubmitChanges()

            Console.WriteLine("== Attaching each story to the new corresponding tag")

            'Loop on StoryTagMeta
            For Each storyTagMeta In d_stories.AP_Stories_Tag_Metas

                Dim storyId = storyTagMeta.StoryId
                Dim tagId = storyTagMeta.TagId

                'Get the new TagId
                Dim tagName = (From t In d_stories.AP_Stories_Tags Where t.StoryTagId = tagId).First.TagName
                Dim tabModuleId = (From s In d_stories.AP_Stories Where s.StoryId = storyId).First.TabModuleId
                Dim storyModuleId = (From m In d_stories.AP_Stories_Modules Where m.TabModuleId = tabModuleId).First.StoryModuleId
                Dim newTagId = (From t In d_stories.AP_Stories_Tags Where t.StoryModuleId = storyModuleId And t.TagName = tagName).First.StoryTagId

                'Set the new TagId on the StoryTagMeta
                storyTagMeta.TagId = newTagId

                Console.WriteLine("Attached story '" + storyId + "' with TagId '" + tagId + "' (tagName '" + tagName + "') to new TagId '" + newTagId + "'")

            Next
            d_stories.SubmitChanges()

            Console.WriteLine("== Deleting old tags attached to Portal instead of Stories module")

            'Delete tags with no StoryModuleId
            Dim tagsToDelete = (From t In d_stories.AP_Stories_Tags Where Not t.StoryModuleId.HasValue)
            d_stories.AP_Stories_Tags.DeleteAllOnSubmit(tagsToDelete)
            d_stories.SubmitChanges()

        End Using
        Console.WriteLine("== Finished. Press any key.")
        Console.ReadKey()
    End Sub

End Module
