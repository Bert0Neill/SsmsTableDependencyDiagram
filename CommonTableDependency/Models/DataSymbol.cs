#region Copyright Bert O'Neill
// Copyright Bert O'Neill All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// bertoneill@yahoo.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Windows.Forms.Diagram;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TableDiagramExtension.Resources;
using static TableDiagramExtension.Models.CustomDiagramTable;

namespace CommonTableDependency.Models
{

    [Serializable]
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Assert, Name = "FullTrust")]
    public class DataSymbol : Group
    {
        public DataSymbol(ArrayList strColumnName, string strTableName, List<string> primaryKey, List<string> foreignKey, List<ColumnData> columnDatas, bool IsCompact, string initialTable)
            : base()
        {
            try
            {
                this.Name = strTableName;
                float rectHeight = (strColumnName.Count * 20) + 40;                
                float maxColumnSize = 0;

                // base table width on longest column name or table name
                if (IsCompact) maxColumnSize = columnDatas.Max(c => c.CompactColumnName.Length);                
                else maxColumnSize = columnDatas.Max(c => c.NonCompactColumnName.Length);

                if (maxColumnSize < strTableName.Length) maxColumnSize = strTableName.Length;

                float tmpColWidth = (maxColumnSize * 8);
                float rectWidth = Math.Max(tmpColWidth, 150); // default is 150 - take the largest to give the table width space for name

                Syncfusion.Windows.Forms.Diagram.Rectangle rect = new Syncfusion.Windows.Forms.Diagram.Rectangle(0, 0, rectWidth, rectHeight);
                if (initialTable != strTableName) rect.FillStyle.Color = Color.WhiteSmoke;
                else rect.FillStyle.Color = Color.LightBlue;
                
                rect.Name = TextStrings.BaseNode;
                rect.TreatAsObstacle = true;

                // label - table name
                Syncfusion.Windows.Forms.Diagram.Label lbl = new Syncfusion.Windows.Forms.Diagram.Label(rect, strTableName);
                lbl.ReadOnly = true;
                lbl.FontStyle.Family = "Arial";
                lbl.FontStyle.Size = 9;
                lbl.FontStyle.Bold = true;
                lbl.Position = Syncfusion.Windows.Forms.Diagram.Position.TopLeft;
                lbl.OffsetX = rectWidth <= 150 ? 65 : rectWidth / 2; // inline calculation for longer table names
                lbl.OffsetY = 18;
                lbl.UpdatePosition = true;
                rect.Labels.Add(lbl);

                this.AppendChild(rect);

                float y_TextNode = 30;
                for (int i = 0; i < columnDatas.Count; i++)
                {
                    var columnField = rectWidth - 40;
                    TextNode txtNode = new TextNode(IsCompact ? columnDatas[i].CompactColumnName : columnDatas[i].NonCompactColumnName, new RectangleF(30, y_TextNode, columnField, 20));
                    txtNode.ReadOnly = true;
                    txtNode.FontStyle.Family = "Arial";
                    txtNode.FontStyle.Size = 7;
                    txtNode.VerticalAlignment = StringAlignment.Center;
                    txtNode.BackgroundStyle.Color = Color.White;
                    txtNode.LineStyle.LineColor = Color.LightGray;
                    txtNode.EditStyle.AllowDelete = false;

                    Syncfusion.Windows.Forms.Diagram.Rectangle symRect = new Syncfusion.Windows.Forms.Diagram.Rectangle(10, y_TextNode, 20, 20);

                    symRect.FillStyle.Color = Color.WhiteSmoke;
                    symRect.FillStyle.Type = FillStyleType.LinearGradient;
                    symRect.LineStyle.LineColor = Color.LightGray;
                    symRect.EditStyle.AllowSelect = false;
                    symRect.EditStyle.AllowDelete = false;

                    this.AppendChild(txtNode);
                    this.AppendChild(symRect);

                    if (primaryKey.Contains(columnDatas[i].CompactColumnName))
                    {
                        Bitmap bmp = new Bitmap(Properties.Resources.DBPrimaryKey);
                        BitmapNode bmpNode = new BitmapNode(bmp, new RectangleF(symRect.BoundingRectangle.X + 5, symRect.BoundingRectangle.Y + 5, symRect.BoundingRectangle.Width - 10, symRect.BoundingRectangle.Height - 10));
                        bmpNode.LineStyle.LineColor = Color.Transparent;
                        bmpNode.EditStyle.AllowSelect = false;
                        this.AppendChild(bmpNode);
                    }

                    if (foreignKey.Contains(columnDatas[i].CompactColumnName))
                    {
                        Bitmap bmp = new Bitmap(Properties.Resources.ForeignKey);
                        BitmapNode bmpNode = new BitmapNode(bmp, new RectangleF(symRect.BoundingRectangle.X + 5, symRect.BoundingRectangle.Y + 5, symRect.BoundingRectangle.Width - 10, symRect.BoundingRectangle.Height - 10));
                        bmpNode.LineStyle.LineColor = Color.Transparent;
                        bmpNode.EditStyle.AllowSelect = false;
                        this.AppendChild(bmpNode);
                    }

                    y_TextNode = y_TextNode + 20;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
