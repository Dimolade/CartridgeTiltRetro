using Eto.Forms;
using Eto.Drawing;

public static class ListBoxUtils
{
    public static void MakeListBoxReorderable(ListBox listBox)
    {
        int dragIndex = -1;

        listBox.MouseDown += (sender, e) =>
        {
            dragIndex = GetIndexFromPosition(listBox, e.Location);
        };

        listBox.MouseUp += (sender, e) =>
        {
            if (dragIndex < 0)
                return;

            int dropIndex = GetIndexFromPosition(listBox, e.Location);

            if (dropIndex >= 0 && dropIndex != dragIndex)
            {
                var item = listBox.Items[dragIndex];
                listBox.Items.RemoveAt(dragIndex);
                listBox.Items.Insert(dropIndex, item);
                listBox.SelectedIndex = dropIndex;
            }

            dragIndex = -1;
        };
    }

    // This helper calculates the index based on mouse Y position
    private static int GetIndexFromPosition(ListBox listBox, PointF location)
    {
        const int itemHeight = 24; // Adjust if needed for your style/theme
        int index = (int)(location.Y / itemHeight);
        return index >= 0 && index < listBox.Items.Count ? index : -1;
    }
}
