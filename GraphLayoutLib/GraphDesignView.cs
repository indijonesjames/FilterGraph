using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VectorMath;

namespace GraphLayoutLib
{
    public partial class GraphDesignView : UserControl
    {

        private GraphDesign graphDesign;
        private bool needsLayout = true;
        private int2 contentOffset = int2.Zero;
        public event Action Changed;
        public event Action<NodeDesign> EditNodeDesign;

        void dirty()
        {
            if (Changed != null) {
                Changed();
            }
        }

        public GraphDesign GraphDesign {
            get {
                return graphDesign;
            }
            set {
                graphDesign = value;
                needsLayout = true;
                Invalidate();
            }
        }

        public GraphDesignView()
        {
            InitializeComponent();
            pinConnectionColor = pinInteriorColor;
        }

        Font nodeLabelFont = new Font("Arial", 12.0f);
        Font pinLabelFont = new Font("Arial", 9.0f);
        Color nodeInteriorColor = Color.FromArgb(64, 64, 64);
        Color nodeLabelColor = Color.White;
        Color nodeOutlineColor = Color.FromArgb(100, 100, 100);
        Color nodeOutlineHoverColor = Color.FromArgb(135, 135, 135);
        Color pinInteriorColor = Color.FromArgb(228, 148, 44);
        Color pinLabelColor = Color.White;
        Color nodeHeaderColor = Color.FromArgb(15, 63, 112);
        Color nodeHeaderSelectedColor = Color.FromArgb(128, 128, 0);
        Color pinFocusColor = Color.Yellow;
        Color pinConnectionColor = Color.FromArgb(200, 200, 200);
        Color nodeOutlineSelectedColor = Color.FromArgb(230, 230, 128);
        Color nodeOutlineSelectedHoverColor = Color.FromArgb(255, 255, 0);
        float pinFocusLineWidth = 2.0f;
        float nodeOutlineWidth = 2.0f;
        int nodeCornerRadius = 8;
        int lineHeight = 24;
        int headerHeight = 24;
        int labelMargin = 4;
        float pinConnectionLineWidth = 6.0f;

        List<Hit> hits = new List<Hit>();

        Point startDrag;
        bool isDragging = false;
        Hit dragHit;
        int2 dragOffset;

        void BeginDrag(Hit hit, Point p)
        {
            dragHit = hit;
            startDrag = p;
            isDragging = true;
        }

        PinDesign focusPin = null;

        int2 previewConnectionStart;
        int2 previewConnectionEnd;
        bool isPreviewConnection = false;
        bool isForward = false;
        Dictionary<object, Rectangle> boundsMetadata = new Dictionary<object, Rectangle>();
        PinDesign inputPin = null;
        PinDesign outputPin = null;


        IEnumerable<NodeDesign> NodeDesigns {
            get {
                foreach (var d in this.graphDesign.nodeDesigns) yield return d;
                if (previewNodeDesign != null) yield return previewNodeDesign;
            }
        }

        List<object> selection = new List<object>();

        private void GraphDesignView_Paint(object sender, PaintEventArgs e)
        {
            if (DesignMode) return;
            if (this.graphDesign == null) return;

            this.hits.Clear();

            if (needsLayout) {
                DoLayout();
                needsLayout = false;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            Brush nodeInteriorBrush = new SolidBrush(nodeInteriorColor);
            Brush nodeLabelBrush = new SolidBrush(nodeLabelColor);
            Pen nodeOutlinePen = new Pen(nodeOutlineColor, nodeOutlineWidth);
            Pen nodeOutlineHoverPen = new Pen(nodeOutlineHoverColor, nodeOutlineWidth);
            Brush pinInteriorBrush = new SolidBrush(pinInteriorColor);
            Brush nodeHeaderBrush = new SolidBrush(nodeHeaderColor);
            Brush nodeHeaderSelectedBrush = new SolidBrush(nodeHeaderSelectedColor);
            Pen pinFocusPen = new Pen(pinFocusColor, pinFocusLineWidth);
            Pen nodeOutlineSelectedHoverPen = new Pen(nodeOutlineSelectedHoverColor, nodeOutlineWidth);
            Pen nodeOutlineSelectedPen = new Pen(nodeOutlineSelectedColor, nodeOutlineWidth);

            // For each Node
            foreach (var nodeDesign in this.NodeDesigns) {
                int2 nodePosition = nodeDesign.position + contentOffset;

                // compute size
                int maxPins = Math.Max(nodeDesign.inputPinDesigns.Count, nodeDesign.outputPinDesigns.Count);
                nodeDesign.size.y = headerHeight + maxPins * lineHeight;

                Rectangle rect = new Rectangle((nodePosition).ToPoint(), nodeDesign.size.ToSize());
                RectangleF layoutRectangle = new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

                var path = RoundedRectangle.Create(rect, nodeCornerRadius);

                g.FillPath(nodeInteriorBrush, path);
                Rectangle rect2 = rect;
                rect2.Height = headerHeight;
                Hit me = null;
                hits.Add(me = new Hit() {
                    bounds = rect,
                    MouseDown = delegate (Point p) {
                        dragOffset = nodeDesign.position - new int2(p.X, p.Y);
                        BeginDrag(me, p);
                        Invalidate();
                        if (!selection.Remove(nodeDesign)) {
                            selection.Clear();
                            selection.Add(nodeDesign);
                        }
                    },
                    MouseDoubleClick = delegate (Point p) {
#if true
                        DoEditNodeDesign(nodeDesign);
#endif
                    },
                    Drag = delegate (Point p) {
                        nodeDesign.position = dragOffset + new int2(p.X, p.Y);
                        dirty();
                        Invalidate();
                    },
                    HoverFocus = delegate () {
                        nodeDesign.flag = true;
                        Invalidate();
                    },
                    HoverBlur = delegate () {
                        nodeDesign.flag = false;
                        Invalidate();
                    }
                });

                var path2 = RoundedRectangle.Create(rect2, nodeCornerRadius, RoundedRectangle.RectangleCorners.TopLeft | RoundedRectangle.RectangleCorners.TopRight);
                g.FillPath(nodeHeaderBrush, path2);

                if (nodeDesign.flag) {
                    if (selection.Contains(nodeDesign)) {
                        g.DrawPath(nodeOutlineSelectedHoverPen, path);
                    } else {
                        g.DrawPath(nodeOutlineHoverPen, path);
                    }
                } else {
                    if (selection.Contains(nodeDesign)) {
                        g.DrawPath(nodeOutlineSelectedPen, path);
                    } else {
                        g.DrawPath(nodeOutlinePen, path);
                    }
                }
                StringFormat format = StringFormat.GenericDefault;
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(nodeDesign.name, this.nodeLabelFont, nodeLabelBrush, rect2.ToRectangleF(), format);

                Rectangle nodeDesignRect = new Rectangle(nodeDesign.position.ToPoint(), nodeDesign.size.ToSize());
                nodeDesignRect.Offset(this.contentOffset.ToPoint());

                // Draw the inputs and outputs
                int index = 0;
                int2 pinSize = new int2(12, 12);

                StringFormat stringFormat = StringFormat.GenericDefault;
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Center;
                int top = nodeDesignRect.Y + headerHeight + lineHeight / 2 - pinSize.y / 2;
                Rectangle pinRect = new Rectangle(nodeDesignRect.X, top, pinSize.x, pinSize.y);
                foreach (var pin in nodeDesign.inputPinDesigns) {
                    boundsMetadata[pin] = pinRect;
                    g.FillRectangle(pinInteriorBrush, pinRect);
                    if (object.ReferenceEquals(pin, focusPin)) {
                        g.DrawRectangle(pinFocusPen, pinRect);
                    }
                    Brush pinLabelBrush = new SolidBrush(pinLabelColor);
                    RectangleF textLayoutRect = new Rectangle(pinRect.Right + labelMargin, pinRect.Y + pinSize.x / 2 - lineHeight / 2, nodeDesignRect.Width / 2 - pinSize.x - labelMargin, lineHeight).ToRectangleF();
                    g.DrawString(pin.name, pinLabelFont, pinLabelBrush, textLayoutRect, stringFormat);

                    Hit h = null;
                    hits.Add(h = new Hit() {
                        bounds = pinRect,
                        MouseDown = delegate (Point p) {
                            BeginDrag(h, p);
                            inputPin = pin;
                            outputPin = null;
                            previewConnectionStart = new int2(h.bounds.X + h.bounds.Width / 2, h.bounds.Y + h.bounds.Height / 2);
                            allowDrop = true;
                            isForward = false;
                        },
                        HoverFocus = delegate () {
                            focusPin = pin;
                            Invalidate();
                        },
                        HoverBlur = delegate () {
                            focusPin = null;
                            Invalidate();
                        },
                        Drag = delegate (Point p) {
                            isPreviewConnection = true;
                            previewConnectionEnd = new int2(p.X, p.Y);
                            Invalidate();
                        },
                        EndDrag = delegate () {
                            isPreviewConnection = false;
                            Invalidate();
                        },
                        PreviewDrop = delegate (Point p) {
                            if (isForward) {
                                previewConnectionStart = new int2(h.bounds.X + h.bounds.Width / 2, h.bounds.Y + h.bounds.Height / 2);
                                return true;
                            } else {
                                return false;
                            }
                        },
                        Drop = delegate () {
                            TogglePinConnection(pin, outputPin);
                            dirty();
                            Invalidate();
                        }
                    });

                    ++index;
                    pinRect.Offset(0, lineHeight);

                }

                index = 0;
                pinRect = new Rectangle(nodeDesignRect.Right - pinSize.x, top, pinSize.x, pinSize.y);
                stringFormat.Alignment = StringAlignment.Far;
                foreach (var pin in nodeDesign.outputPinDesigns) {
                    boundsMetadata[pin] = pinRect;
                    g.FillRectangle(pinInteriorBrush, pinRect);
                    if (object.ReferenceEquals(pin, focusPin)) {
                        g.DrawRectangle(pinFocusPen, pinRect);
                    }
                    Brush pinLabelBrush = new SolidBrush(pinLabelColor);
                    RectangleF textLayoutRect = new Rectangle(pinRect.Left - (nodeDesignRect.Width / 2 - pinSize.x) - labelMargin, pinRect.Y + pinSize.x / 2 - lineHeight / 2, nodeDesignRect.Width / 2 - pinSize.x - labelMargin, lineHeight).ToRectangleF();
                    g.DrawString(pin.name, pinLabelFont, pinLabelBrush, textLayoutRect, stringFormat);

                    Hit h = null;
                    hits.Add(h = new Hit() {
                        bounds = pinRect,
                        MouseDown = delegate (Point p) {
                            BeginDrag(h, p);
                            inputPin = null;
                            outputPin = pin;
                            previewConnectionEnd = new int2(h.bounds.X + h.bounds.Width / 2, h.bounds.Y + h.bounds.Height / 2);
                            allowDrop = true;
                            isForward = true;
                        },
                        HoverFocus = delegate () {
                            focusPin = pin;
                            Invalidate();
                        },
                        HoverBlur = delegate () {
                            focusPin = null;
                            Invalidate();
                        },
                        Drag = delegate (Point p) {
                            isPreviewConnection = true;
                            previewConnectionStart = new int2(p.X, p.Y);
                            Invalidate();
                        },
                        EndDrag = delegate () {
                            isPreviewConnection = false;
                            Invalidate();
                        },
                        PreviewDrop = delegate (Point p) {
                            if (!isForward) {
                                previewConnectionEnd = new int2(h.bounds.X + h.bounds.Width / 2, h.bounds.Y + h.bounds.Height / 2);
                                return true;
                            } else {
                                return false;
                            }
                        },
                        Drop = delegate () {
                            TogglePinConnection(inputPin, pin);
                            Invalidate();
                        }
                    });

                    ++index;
                    pinRect.Offset(0, lineHeight);
                }
            }
            foreach (var pinConnectionDesign in graphDesign.pinConnectionDesigns) {
                Rectangle outputBounds;
                if (boundsMetadata.TryGetValue(pinConnectionDesign.outputPinDesign, out outputBounds)) {
                    Rectangle inputBounds;
                    if (boundsMetadata.TryGetValue(pinConnectionDesign.inputPinDesign, out inputBounds)) {
                        DrawConnection(g,
                            new int2(inputBounds.X + inputBounds.Width / 2, inputBounds.Y + inputBounds.Height / 2),
                            new int2(outputBounds.X + outputBounds.Width / 2, outputBounds.Y + outputBounds.Height / 2)
                            );
                    }
                }
            }
            if (isPreviewConnection) {
                DrawConnection(g, previewConnectionStart, previewConnectionEnd);
            }

        }

        private void TogglePinConnection(PinDesign inputPin, PinDesign outputPin)
        {
            int numRemoved = this.graphDesign.pinConnectionDesigns.RemoveAll(s => object.ReferenceEquals(s.inputPinDesign, inputPin) && object.ReferenceEquals(s.outputPinDesign, outputPin));
            if (numRemoved == 0) {
                this.graphDesign.pinConnectionDesigns.Add(new PinConnectionDesign() { inputPinDesign = inputPin, outputPinDesign = outputPin });
            }
            Invalidate();
        }
        private void DrawConnection(Graphics g, int2 previewConnectionStart, int2 previewConnectionEnd)
        {
            Pen pen = new Pen(pinConnectionColor, pinConnectionLineWidth);
            pen.StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
            int d = (int)Math.Sqrt((double)(previewConnectionEnd - previewConnectionStart).Magnitude2() * 0.5);
            Pen pen0 = new Pen(Color.Black, pinConnectionLineWidth);
            pen0.StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            pen0.EndCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
            pen0.Alignment = System.Drawing.Drawing2D.PenAlignment.Right;
            Point p1 = previewConnectionStart.ToPoint();
            Point p2 = (previewConnectionStart + new int2(-d, 0)).ToPoint();
            Point p3 = (previewConnectionEnd + new int2(d, 0)).ToPoint();
            Point p4 = previewConnectionEnd.ToPoint();
            for (int dx = -1; dx <= 1; ++dx) {
                for (int dy = -1; dy <= 1; ++dy) {
                    g.TranslateTransform(dx, dy);
                    g.DrawBezier(pen0, p1, p2, p3, p4);
                    g.ResetTransform();
                }
            }
            g.DrawBezier(pen, p1, p2, p3, p4);
        }

        public void Center()
        {
            needsLayout = true;
            Invalidate();
        }

        private void DoLayout()
        {
            int2 min = new int2(int.MaxValue, int.MaxValue);
            int2 max = new int2(int.MinValue, int.MinValue);
            if (this.graphDesign.nodeDesigns.Count() == 0) {
                contentOffset = int2.Zero;
                return;
            }
            foreach (var nodeDesign in this.graphDesign.nodeDesigns) {
                int2 lo = nodeDesign.position;
                int2 hi = nodeDesign.position + nodeDesign.size;
                if (lo.x < min.x) {
                    min.x = lo.x;
                }
                if (hi.x > max.x) {
                    max.x = hi.x;
                }
                if (lo.y < min.y) {
                    min.y = lo.y;
                }
                if (hi.y > max.y) {
                    max.y = hi.y;
                }
            }
            int2 contentSize = max - min;
            int2 windowSize = new int2(this.Width, this.Height);
            contentOffset = ((windowSize - contentSize) / 2) - min;
        }

        Point firstScroll;
        bool isScrolling = false;
        void ScrollStart(Point point)
        {
            firstScroll = point;
            firstContentOffset = contentOffset;
        }

        DateTime lastMouseDownTime = DateTime.Now;


        private void GraphDesignView_MouseDown(object sender, MouseEventArgs e)
        {
            DateTime now = DateTime.Now;
            bool isDoubleClick = (now - lastMouseDownTime).TotalMilliseconds <= SystemInformation.DoubleClickTime;
            lastMouseDownTime = now;
            allowDrop = false;
            Hit hit = GetHit(e.Location);
            if (hit != null) {
                // we hit something!
                if (isDoubleClick) {
                    if (hit.MouseDoubleClick != null) {
                        hit.MouseDoubleClick(e.Location);
                    }
                } else {
                    if (hit.MouseDown != null) {
                        hit.MouseDown(e.Location);
                    }
                }
            } else {
                // time to scroll!
                ScrollStart(e.Location);
                isScrolling = true;
                didScroll = false;
            }
        }

        private void GraphDesignView_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging) {
                if (dragHit.EndDrag != null) {
                    dragHit.EndDrag();
                }
                if (allowDrop) {
                    if (dropHit != null) {
                        if (dropHit.PreviewDrop != null) {
                            bool accept = dropHit.PreviewDrop(e.Location);
                            if (accept) {
                                if (dropHit.Drop != null) {
                                    dropHit.Drop();
                                }
                            }
                        }
                    }
                }
            }
            if (isScrolling) {
                if (!didScroll) {
                    selection.Clear();
                    Invalidate();
                }
            }
            isDragging = false;
            isScrolling = false;
            allowDrop = false;
        }

        Hit hoverFocusHit = null;
        Hit dropHit;
        bool allowDrop = false;

        int2 firstContentOffset;
        bool didScroll = false;

        private void GraphDesignView_MouseMove(object sender, MouseEventArgs e)
        {
            Hit previousHoverFocusHit = hoverFocusHit;
            if (isDragging) {
                if (dragHit.Drag != null) {
                    dragHit.Drag(e.Location);
                }
                if (allowDrop) {
                    dropHit = GetHit(e.Location);
                    if (dropHit != null) {
                        if (dropHit.PreviewDrop != null) {
                            bool accept = dropHit.PreviewDrop(e.Location);
                            if (accept) {
                                hoverFocusHit = dropHit;
                            }
                        }
                    } else {
                        hoverFocusHit = null;
                    }
                }
            } else {
                hoverFocusHit = GetHit(e.Location);
            }
            if (isScrolling) {
                contentOffset = firstContentOffset + new int2(e.X - firstScroll.X, e.Y - firstScroll.Y);
                didScroll = true;
                Invalidate();
            }
            if (!object.ReferenceEquals(previousHoverFocusHit, hoverFocusHit)) {
                if (previousHoverFocusHit != null) {
                    if (previousHoverFocusHit.HoverBlur != null) {
                        previousHoverFocusHit.HoverBlur();
                    }
                }
                if (hoverFocusHit != null) {
                    if (hoverFocusHit.HoverFocus != null) {
                        hoverFocusHit.HoverFocus();
                    }
                }
            }
        }

        private void GraphDesignView_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        Hit GetHit(Point point)
        {
            foreach (var hit in hits.Reverse<Hit>()) {
                if (hit.bounds.Contains(point)) {
                    return hit;
                }
            }
            return null;
        }

        private void GraphDesignView_DragDrop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DragDrop");
            var formats = e.Data.GetFormats();
            Debug.WriteLine(string.Join(", ", formats));
            if (e.Data.GetDataPresent("System.RuntimeType")) {
                object data = e.Data.GetData("System.RuntimeType");
                Type type = data as Type;
                Debug.WriteLine(type.Name);
                if (previewNodeDesign != null) {
                    this.graphDesign.nodeDesigns.Add(previewNodeDesign);
                    previewNodeDesign = null;
                }
            }
        }

        private void GraphDesignView_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DragEnter");
            var formats = e.Data.GetFormats();
            Debug.WriteLine(string.Join(", ", formats));
            if (e.Data.GetDataPresent("System.RuntimeType")) {
                object data = e.Data.GetData("System.RuntimeType");
                Type type = data as Type;
                Debug.WriteLine(type.Name);
#if false
                previewNodeDesign = NodeDesign.BuildNodeDesignFromType(type);
#endif
                e.Effect = DragDropEffects.Copy;
            }
        }



        private void GraphDesignView_DragLeave(object sender, EventArgs e)
        {
            Debug.WriteLine("DragLeave");
        }

        NodeDesign previewNodeDesign = null;
        private void GraphDesignView_DragOver(object sender, DragEventArgs e)
        {
            // move the preview designer to this spot
            if (previewNodeDesign != null) {
                previewNodeDesign.position = this.PointToClient(new Point(e.X, e.Y)).ToInt2() - this.contentOffset;
                ////= new int2( e.X, e.Y ) - this.contentOffset;
                Invalidate();
            }
            //Debug.WriteLine( "DragOver" );
        }

        private void GraphDesignView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            Debug.WriteLine("GiveFeedback");
        }

        private void GraphDesignView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            Debug.WriteLine("QueryContinueDrag");
        }

        private void GraphDesignView_KeyDown(object sender, KeyEventArgs e)
        {
            //Debug.WriteLine( string.Format( "Key Down: {0}", e.KeyCode ) );
            if (e.KeyCode == Keys.Delete) {
                DoDeleteSelection();
            }
        }

        void DoDeleteSelection()
        {
            foreach (object obj in selection) {
                if (obj is NodeDesign) {
                    NodeDesign nodeDesign = (NodeDesign)obj;
                    // Remove all connections that point to this node.
                    this.graphDesign.pinConnectionDesigns.RemoveAll(c => nodeDesign.inputPinDesigns.Concat(nodeDesign.outputPinDesigns).Any(x =>
                      object.ReferenceEquals(c.inputPinDesign, x) || object.ReferenceEquals(c.outputPinDesign, x)));
                    this.graphDesign.nodeDesigns.Remove(obj as NodeDesign);
                }
            }
            dirty();
            Invalidate();
        }

        void DoEditNodeDesign(NodeDesign nodeDesign)
        {
            if (EditNodeDesign != null) {
                EditNodeDesign(nodeDesign);
            }
        }

        //    FillInTheBlanksForm form = new FillInTheBlanksForm();
        //    int index = 0;
        //    ConstructorInfo constructorInfo = nodeDesign.constructorInfo;
        //    if (constructorInfo == null) {
        //        // select a constructor
        //        constructorInfo = nodeDesign.nodeType.GetConstructor(new Type[] { });
        //        if (constructorInfo == null) {
        //            var r = nodeDesign.nodeType.GetConstructors();
        //            if (r.Length > 0) {
        //                constructorInfo = r[0];
        //            }
        //        }
        //        nodeDesign.constructorInfo = constructorInfo;
        //    }
        //    if (constructorInfo == null) {
        //        throw new DesignException("No appropriate constructor available.");
        //    }
        //    foreach (var p in constructorInfo.GetParameters()) {
        //        object defaultObj = null;
        //        if (nodeDesign.constructorParameters != null) {
        //            defaultObj = nodeDesign.constructorParameters.GetValue(index);
        //        }
        //        if (p.ParameterType.IsValueType) {
        //            defaultObj = Activator.CreateInstance(p.ParameterType);
        //        }
        //        form.AddBlank(p.Name, p.ParameterType, defaultObj);
        //    }
        //    form.Values = nodeDesign.constructorParameters;
        //    form.ShowDialog();
        //    nodeDesign.constructorParameters = form.Values;


    }

}
