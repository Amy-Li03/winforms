﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms.ButtonInternal
{
    internal sealed class CheckBoxStandardAdapter : CheckBoxBaseAdapter
    {
        internal CheckBoxStandardAdapter(ButtonBase control) : base(control) { }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            if (Control.Appearance == Appearance.Button)
            {
                ButtonAdapter.PaintUp(e, Control.CheckState);
            }
            else
            {
                ColorData colors = PaintRender(e).Calculate();
                LayoutData layout = Layout(e).Layout();
                PaintButtonBackground(e, Control.ClientRectangle, null);

                if (!layout.options.EverettButtonCompat)
                {
                    layout.textBounds.Offset(-1, -1);
                }
                layout.imageBounds.Offset(-1, -1);

                AdjustFocusRectangle(layout);

                if (!string.IsNullOrEmpty(Control.Text))
                {
                    //minor adjustment to make sure the appearance is exactly the same as Win32 app.
                    int focusRectFixup = layout.focus.X & 0x1; // if it's odd, subtract one pixel for fixup.
                    if (!Application.RenderWithVisualStyles)
                    {
                        focusRectFixup = 1 - focusRectFixup;
                    }
                    layout.focus.Offset(-(focusRectFixup + 1), -2);
                    layout.focus.Width = layout.textBounds.Width + layout.imageBounds.Width - 1;
                    layout.focus.Intersect(layout.textBounds);

                    if (layout.options.TextAlign != LayoutUtils.AnyLeft && layout.options.UseCompatibleTextRendering && layout.options.Font.Italic)
                    {
                        // fixup for GDI+ text rendering.
                        layout.focus.Width += 2;
                    }
                }

                PaintImage(e, layout);
                DrawCheckBox(e, layout);
                PaintField(e, layout, colors, colors.WindowText, true);
            }
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            if (Control.Appearance == Appearance.Button)
            {
                ButtonAdapter.PaintDown(e, Control.CheckState);
            }
            else
            {
                PaintUp(e, state);
            }
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            if (Control.Appearance == Appearance.Button)
            {
                ButtonAdapter.PaintOver(e, Control.CheckState);
            }
            else
            {
                PaintUp(e, state);
            }
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            if (Control.Appearance == Appearance.Button)
            {
                ButtonStandardAdapter adapter = new ButtonStandardAdapter(Control);
                return adapter.GetPreferredSizeCore(proposedSize);
            }
            else
            {
                LayoutOptions options = default;
                using (var screen = GdiCache.GetScreenHdc())
                using (PaintEventArgs pe = new PaintEventArgs(screen, new Rectangle()))
                {
                    options = Layout(pe);
                }

                return options.GetPreferredSizeCore(proposedSize);
            }
        }

        #region Layout

        private new ButtonStandardAdapter ButtonAdapter
        {
            get
            {
                return ((ButtonStandardAdapter)base.ButtonAdapter);
            }
        }

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonStandardAdapter(Control);
        }

        protected override LayoutOptions Layout(PaintEventArgs e)
        {
            LayoutOptions layout = CommonLayout();
            layout.CheckPaddingSize = 1;
            layout.EverettButtonCompat = !Application.RenderWithVisualStyles;

            if (Application.RenderWithVisualStyles)
            {
                using var screen = GdiCache.GetScreenHdc();
                layout.CheckSize = CheckBoxRenderer.GetGlyphSize(
                    screen,
                    CheckBoxRenderer.ConvertFromButtonState(
                        GetState(),
                        true,
                        Control.MouseIsOver),
                    Control.HandleInternal).Width;
            }
            else
            {
                if (DpiHelper.IsPerMonitorV2Awareness)
                {
                    layout.CheckSize = Control.LogicalToDeviceUnits(layout.CheckSize);
                }
                else
                {
                    layout.CheckSize = (int)(layout.CheckSize * GetDpiScaleRatio());
                }
            }

            return layout;
        }

        #endregion
    }
}
