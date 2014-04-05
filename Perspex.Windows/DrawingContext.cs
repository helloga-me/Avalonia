﻿// -----------------------------------------------------------------------
// <copyright file="DrawingContext.cs" company="Steven Kirk">
// Copyright 2013 MIT Licence. See licence.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Perspex.Windows
{
    using System;
    using System.Reactive.Disposables;
    using Perspex.Media;
    using Perspex.Windows.Media;
    using SharpDX;
    using SharpDX.Direct2D1;
    using Matrix = Perspex.Media.Matrix;

    /// <summary>
    /// Draws using Direct2D1.
    /// </summary>
    public class DrawingContext : IDrawingContext, IDisposable
    {
        /// <summary>
        /// The Direct2D1 render target.
        /// </summary>
        private RenderTarget renderTarget;

        /// <summary>
        /// The DirectWrite factory.
        /// </summary>
        private SharpDX.DirectWrite.Factory directWriteFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        /// <param name="renderTarget">The render target to draw to.</param>
        /// <param name="directWriteFactory">The DirectWrite factory.</param>
        public DrawingContext(
            RenderTarget renderTarget,
            SharpDX.DirectWrite.Factory directWriteFactory)
        {
            this.renderTarget = renderTarget;
            this.directWriteFactory = directWriteFactory;
            this.renderTarget.BeginDraw();
        }

        /// <summary>
        /// Ends a draw operation.
        /// </summary>
        public void Dispose()
        {
            this.renderTarget.EndDraw();
        }

        /// <summary>
        /// Draws the outline of a rectangle.
        /// </summary>
        /// <param name="pen">The pen.</param>
        /// <param name="rect">The rectangle bounds.</param>
        public void DrawRectange(Pen pen, Rect rect)
        {
            using (SharpDX.Direct2D1.SolidColorBrush brush = this.Convert(pen.Brush))
            {
                this.renderTarget.DrawRectangle(
                    this.Convert(rect),
                    brush,
                    (float)pen.Thickness);
            }
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="foreground">The foreground brush.</param>
        /// <param name="rect">The output rectangle.</param>
        /// <param name="text">The text.</param>
        public void DrawText(Perspex.Media.Brush foreground, Rect rect, FormattedText text)
        {
            if (!string.IsNullOrEmpty(text.Text))
            {
                using (SharpDX.Direct2D1.SolidColorBrush brush = this.Convert(foreground))
                using (SharpDX.DirectWrite.TextFormat format = TextService.Convert(this.directWriteFactory, text))
                {
                    this.renderTarget.DrawText(
                        text.Text,
                        format,
                        this.Convert(rect),
                        brush);
                }
            }
        }

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="rect">The rectangle bounds.</param>
        public void FillRectange(Perspex.Media.Brush brush, Rect rect)
        {
            using (SharpDX.Direct2D1.SolidColorBrush b = this.Convert(brush))
            {
                this.renderTarget.FillRectangle(
                    new RectangleF(
                        (float)rect.X,
                        (float)rect.Y,
                        (float)rect.Width,
                        (float)rect.Height),
                    b);
            }
        }

        /// <summary>
        /// Pushes a matrix transformation.
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <returns>A disposable used to undo the transformation.</returns>
        public IDisposable PushTransform(Matrix matrix)
        {
            Matrix3x2 m3x2 = this.Convert(matrix);
            Matrix3x2 transform = this.renderTarget.Transform * m3x2;
            this.renderTarget.Transform = transform;

            return Disposable.Create(() =>
            {
                m3x2.Invert();
                this.renderTarget.Transform = transform * m3x2;
            });
        }

        /// <summary>
        /// Converts a brush to Direct2D.
        /// </summary>
        /// <param name="brush">The brush to convert.</param>
        /// <returns>The Direct2D brush.</returns>
        private SharpDX.Direct2D1.SolidColorBrush Convert(Perspex.Media.Brush brush)
        {
            Perspex.Media.SolidColorBrush solidColorBrush = brush as Perspex.Media.SolidColorBrush;

            if (solidColorBrush != null)
            {
                return new SharpDX.Direct2D1.SolidColorBrush(
                    this.renderTarget, 
                    this.Convert(solidColorBrush.Color));
            }
            else
            {
                return new SharpDX.Direct2D1.SolidColorBrush(
                    this.renderTarget,
                    new Color4());
            }
        }

        /// <summary>
        /// Converts a color to Direct2D.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The Direct2D color.</returns>
        private Color4 Convert(Perspex.Media.Color color)
        {
            return new Color4(
                (float)(color.R / 255.0),
                (float)(color.G / 255.0),
                (float)(color.B / 255.0),
                (float)(color.A / 255.0));
        }

        /// <summary>
        /// Converts a <see cref="Matrix"/> to a Direct2D <see cref="Matrix3x2"/>
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix"/>.</param>
        /// <returns>The <see cref="Matrix3x2"/>.</returns>
        private Matrix3x2 Convert(Matrix matrix)
        {
            return new Matrix3x2(
                (float)matrix.M11,
                (float)matrix.M12,
                (float)matrix.M21,
                (float)matrix.M22,
                (float)matrix.OffsetX,
                (float)matrix.OffsetY);
        }

        /// <summary>
        /// Converts a <see cref="Rect"/> to a <see cref="RectangleF"/>
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/>.</param>
        /// <returns>The <see cref="RectangleF"/>.</returns>
        private RectangleF Convert(Rect rect)
        {
            return new RectangleF(
                (float)rect.X,
                (float)rect.Y,
                (float)rect.Width,
                (float)rect.Height);
        }
    }
}
