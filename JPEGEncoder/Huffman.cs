using System.Text;

public static class Huffman{
    public static string Encode<T>(T[] data) where T: notnull
    {
        Dictionary<T,long> counts = new();
        for (int i = 0; i < data.Length; i++)
        {
            if(counts.ContainsKey(data[i]))counts[data[i]]++;
            else counts.Add(data[i],1);
        }

        var queue = new PriorityQueue<long, Node<T>>(x=>x.Count,false);

        foreach (var item in counts)
        {
            queue.Enqueue(new Node<T>(item.Key, item.Value));
        }

        while (queue.Count>1)
        {
            var n1 = queue.Dequeue();
            var n2 = queue.Dequeue();
            queue.Enqueue(new Node<T>(n1,n2));
        }

        var root = queue.Dequeue();

        var stack = new Stack<Node<T>>();
        stack.Push(root);

        Dictionary<T,StringBuilder> codeMap = new();
        while(stack.Any()){
            var current = stack.Pop();
            if(current.Children == null){
                codeMap.Add(current.Key, current.CodeBuilder);
            }
            else{
                var code = current.CodeBuilder;
                current.Children[0].CodeBuilder.Append(code).Append("0");
                current.Children[1].CodeBuilder.Append(code).Append("1");
                stack.Push(current.Children[0]);
                stack.Push(current.Children[1]);
            }
        }

        StringBuilder ans = new();

        foreach (var item in data)
        {
            ans.Append(codeMap[item]);
        }

        return ans.ToString();
    }
}

public class Node<T> where T:notnull
{
    public Node(T key, long count){
        Key = key;
        Count = count;
    }

    public Node(Node<T> n1, Node<T> n2){
        if(n2.Count<n1.Count){
            (n1,n2) = (n2,n1);
        }
            Children = [n1,n2];
            Count = n1.Count + n2.Count;
        
    }

    public StringBuilder CodeBuilder{get;} = new();
    public T Key{get;}
    public long Count{get;}
    public Node<T>[]? Children{get;}
    public bool IsEdge => Children == null;
}