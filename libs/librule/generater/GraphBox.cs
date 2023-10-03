using libgraph;
using librule.targets.code;
using librule.utils;

namespace librule.generater
{
    abstract class GraphBox<TMetadata, TAction> : IProductionGraph<TMetadata>
    {
        private ActionTree mActionTree;
        private List<GraphEdge<TMetadata>> mEdges;
        private List<GraphState<TMetadata>> mStates;
        private LevelCollection<List<GraphEdge<TMetadata>>> mCollectPaths;
        private Dictionary<int, IList<TAction>> mNumberAction;

        IEnumerable<GraphState<TMetadata>> IGraph<GraphState<TMetadata>, GraphEdge<TMetadata>>.Vertices => mStates;

        public IReadOnlyList<GraphState<TMetadata>> States => mStates;

        public IReadOnlyList<GraphEdge<TMetadata>> Edges => mEdges;

        public Dictionary<int, IList<TAction>> Actions => mNumberAction;

        public IList<GraphFigure<TMetadata, TAction>> Figures { get; }

        public List<GrapBoxCallback> Callback { get; }

        public GraphState<TMetadata> Exit { get; }

        public abstract Lexicon Lexicon { get; }

        public GraphBox()
        {
            mCollectPaths = new LevelCollection<List<GraphEdge<TMetadata>>>();
            mEdges = new List<GraphEdge<TMetadata>>();
            mStates = new List<GraphState<TMetadata>>();

            mNumberAction = new Dictionary<int, IList<TAction>>();
            mActionTree = new ActionTree();

            Figures = new List<GraphFigure<TMetadata, TAction>>();
            Callback = new List<GrapBoxCallback>();
            Exit = State();
        }

        public abstract GraphEdgeValue GetMissValue();

        public abstract TMetadata CreateMetadata(GraphEdge<TMetadata> edge, TAction action, bool skipToken);

        public TMetadata MergeMetadatas(params TMetadata[] metadatas)
        {
            return MergeMetadatas(metadatas as IEnumerable<TMetadata>);
        }

        public abstract TMetadata MergeMetadatas(IEnumerable<TMetadata> metadatas);

        public abstract void TokenEdge(GraphEdge<TMetadata> edge, ushort token);

        public abstract GraphTable<TMetadata> Tabulate();

        public virtual GraphTable<TMetadata> Tabulate(GraphFigure<TMetadata, TAction> figure)
        {
            throw new NotImplementedException();
        }

        public GraphFigure<TMetadata, TAction> Figure(string figureName)
        {
            var figure = new GraphFigure<TMetadata, TAction>((ushort)mStates.Count, this, figureName);
            Figures.Add(figure);
            mStates.Add(figure);
            return figure;
        }

        public GraphFigure<TMetadata, TAction> TemporaryFigure()
        {
            return new GraphFigure<TMetadata, TAction>(1, this, "Temporary", new GraphState<TMetadata>(0, GraphStateFlags.None));
        }

        public GraphState<TMetadata> State()
        {
            var state = new GraphState<TMetadata>((ushort)States.Count, GraphStateFlags.None);
            mStates.Add(state);
            return state;
        }

        public GraphState<TMetadata> Empty()
        {
            var state = new GraphState<TMetadata>((ushort)States.Count, GraphStateFlags.Empty);
            mStates.Add(state);
            return state;
        }

        public GraphEdge<TMetadata> Edge(GraphState<TMetadata> left, GraphState<TMetadata> right, GraphState<TMetadata> subset, GraphEdgeValue value, TMetadata metadata)
        {
            var edge = new GraphEdge<TMetadata>(left, right, value, metadata);
            edge.Subset = subset;
            mEdges.Add(edge);

            if (mCollectPaths.Count > 0)
                mCollectPaths.Current.Add(edge);

            return edge;
        }

        public GraphEdge<TMetadata> Edge(GraphState<TMetadata> left, GraphState<TMetadata> right, GraphEdgeValue value, TMetadata metadata)
        {
            return Edge(left, right, null, value, metadata);
        }

        public void AddRange(IEnumerable<GraphEdge<TMetadata>> edges)
        {
            foreach (var edge in edges)
            {
                if (!mStates.Contains(edge.Source))
                    mStates.Add(edge.Source);

                if (!mStates.Contains(edge.Target))
                    mStates.Add(edge.Target);

                if (edge.Subset != null && !mStates.Contains(edge.Subset))
                    mStates.Add(edge.Subset);
            }

            mEdges.AddRange(edges);
        }

        public bool Remove(GraphFigure<TMetadata, TAction> figure) 
        {
            if (Figures.Contains(figure))
            {
                if (Figures.Remove(figure))
                {
                    foreach (var edge in figure.Edges)
                        Remove(edge);

                    foreach (var state in figure.States)
                        mStates.Remove(state);

                    return true;
                }
            }

            return false;
        }

        public bool Remove(GraphEdge<TMetadata> edge)
        {
            return mEdges.Remove(edge);
        }

        internal void StartCollect(List<GraphEdge<TMetadata>> buffer = null)
        {
            mCollectPaths.Push(buffer ?? new List<GraphEdge<TMetadata>>());
        }

        internal List<GraphEdge<TMetadata>> EndCollect(CollectMode mode = CollectMode.Default)
        {
            var list = mCollectPaths.Pop();

            switch (mode)
            {
                case CollectMode.Default:
                    if (mCollectPaths.Count > 0)
                        mCollectPaths.Current.AddRange(list);
                    break;
                case CollectMode.Last:
                    if (mCollectPaths.Count > 0)
                        mCollectPaths.Current.Add(list[^1]);
                    break;
            }

            return list;
        }

        public abstract int GetMetadataCompreValue(TMetadata metadata);

        public virtual TMetadata GetDefaultMetadata() => default;

        public IEnumerable<GraphEdge<TMetadata>> GetLefts(GraphState<TMetadata> state)
        {
            return mEdges.AsParallel().Where(x => x.Target == state);
        }

        public IEnumerable<GraphEdge<TMetadata>> GetRights(GraphState<TMetadata> state)
        {
            return mEdges.AsParallel().Where(x => x.Source == state);
        }

        public short GetActionNumber(params TAction[] actions) 
        {
            return GetActionNumber(actions as IList<TAction>);
        }

        public short GetActionNumber(IList<TAction> actions)
        {
            var v = mActionTree.Get(actions);
            mNumberAction[v] = actions;
            return v;
        }

        public IList<TAction> GetAction(short num)
        {
            return mNumberAction.ContainsKey(num) ? mNumberAction[num] : new TAction[0];
        }

        internal void CreateCodeCallback(Func<object, CodeTargetVisitor, string> callback, object sender) 
        {
            Callback.Add(new GrapBoxCallback(callback, sender));
        }

        class ActionTree
        {
            private short mNextIndex = 1;
            private readonly ActionTreeNode mRoot = new ActionTreeNode(0);

            public short Get(IEnumerable<TAction> actions)
            {
                ActionTreeNode currNode = mRoot;

                foreach (TAction action in actions)
                {
                    if (currNode.TryGetValue(action, out ActionTreeNode child))
                    {
                        currNode = child;
                    }
                    else
                    {
                        child = new ActionTreeNode(mNextIndex++);
                        currNode.Add(action, child);
                        currNode = child;
                    }
                }

                return currNode.Value;
            }

            class ActionTreeNode
            {
                private readonly Dictionary<TAction, ActionTreeNode> mTree = new Dictionary<TAction, ActionTreeNode>();

                public ActionTreeNode(short value)
                {
                    Value = value;
                }

                public short Value { get; }

                public bool TryGetValue(TAction key, out ActionTreeNode value)
                {
                    return mTree.TryGetValue(key, out value);
                }

                public void Add(TAction key, ActionTreeNode value)
                {
                    mTree.Add(key, value);
                }
            }
        }
    }
}
