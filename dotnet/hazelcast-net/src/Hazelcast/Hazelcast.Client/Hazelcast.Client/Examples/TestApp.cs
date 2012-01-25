using System;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.Client.IO;
using System.Threading;


namespace Hazelcast.Client.Examples
{
	
	public interface LineReader {
	    String readLine();
	}
	
	public class DefaultLineReader: LineReader {
		public String readLine(){
            return Console.ReadLine();
        }
    }
	
	
	public class TestApp : MessageListener<Object>, EntryListener<Object, Object>, ItemListener<Object>   
	{
		
		private ISet<Object> set = null;
		
		private IList<Object> list = null;
		
		private IQueue<Object> queue = null;

    	private ITopic<Object> topic = null;

    	private IMap<Object, Object> map = null;
		
		
		private volatile HazelcastClient hazelcast;
		
		private volatile LineReader lineReader;
		
		private volatile bool running = false;
		
		private bool silent = false;

    	private bool echo = false;
		
		private String _namespace = "default";
		
		public TestApp (HazelcastClient client)
		{
			this.hazelcast = client;
		}
		
		public static void Main (){
			HazelcastClient client = HazelcastClient.newHazelcastClient("dev", "dev-pass", "localhost");
			TestApp testApp = new TestApp(client);
			Console.WriteLine("Starting the TestApp");
			testApp.start();
		}
		
		
		public IQueue<Object> getQueue() {
        	queue = hazelcast.getQueue<Object>(_namespace);
        	return queue;
    	}

	    public ITopic<Object> getTopic() {
	     	topic = hazelcast.getTopic<Object>(_namespace);
	        return topic;
	    }

	    public IMap<Object, Object> getMap() {
	        map = hazelcast.getMap<Object, Object>(_namespace);
	        return map;
	    }
		
		public ISet<Object> getSet() {
	        set = hazelcast.getSet<Object>(_namespace);
	        return set;
	    }
		
		public IList<Object> getList() {
	        list = hazelcast.getList<Object>(_namespace);
	        return list;
	    }
		
		
		public void stop() {
	        running = false;
	    }
	
	    public void start(){
	        if (lineReader == null) {
	            lineReader = new DefaultLineReader();
	        }
			println("Initiated the reader");
	        running = true;
	        while (running) {
				print("hazelcast[" + _namespace + "] > ");
	            String command = lineReader.readLine();
	            handleCommand(command);
	        }
	    }
		
		
		
		protected void handleCommand(String command) 
		{
			if (echo) println(command);
			if (command == null || command.StartsWith("//"))
			    return;
			command = command.Trim();
			if (command == null || command.Length == 0) {
			    return;
			}
			String first = command;
			int spaceIndex = command.IndexOf(' ');
			String[] argsSplit = command.Split(' ');
			String[] args = new String[argsSplit.Length];
			for (int i = 0; i < argsSplit.Length; i++) {
			    args[i] = argsSplit[i].Trim();
			}
			if (spaceIndex != -1) {
			    first = args[0];
			}
			if (command.StartsWith("help")) {
			    handleHelp(command);
			} else if (first.StartsWith("#") && first.Length > 1) {
			    int repeat = Int32.Parse(first.Substring(1));
			    DateTime t0 = System.DateTime.Now;
			    for (int i = 0; i < repeat; i++) {
			        handleCommand(command.Substring(first.Length).Replace("\\$i", "" + i));
			    }
			    println("ops/s = " + repeat * 1000 / (System.DateTime.Now - t0).TotalMilliseconds);
			    return;
			} else if (first.StartsWith("&") && first.Length > 1) {
			    //Not implemented!
			} else if (first.StartsWith("@")) {
			    if (first.Length == 1) {
			        println("usage: @<file-name>");
			        return;
			    }
			    //Not implemented!
			} else if (command.IndexOf(";") != -1) {
				String[] commands = command.Split (';');
			    foreach(String c in commands)
			        handleCommand(c);
			  
			    return;
			} else if ("silent".Equals(first)) {
			    silent = Boolean.Parse(args[1]);
			} else if ("restart".Equals(first)) {
			    //hazelcast.restart();
			} else if ("shutdown".Equals(first)) {
			    //hazelcast.shutdown() ;
			} else if ("echo".Equals(first)) {
			    echo = Boolean.Parse(args[1]);
			    println("echo: " + echo);
			} else if ("ns".Equals(first)) {
			    if (args.Length > 1) {
			        _namespace = args[1];
			        println("namespace: " + _namespace);
			    }
			} else if ("whoami".Equals(first)) {
			    //println(hazelcast.getCluster().getLocalMember());
			} else if ("who".Equals(first)) {
			    //println(hazelcast.getCluster());
			} else if (first.IndexOf("ock") != -1 && first.IndexOf(".") == -1) {
			    handleLock(args);
			} else if (first.IndexOf(".size") != -1) {
			    handleSize(args);
			} else if (first.IndexOf(".clear") != -1) {
			    handleClear(args);
			} else if (first.IndexOf(".destroy") != -1) {
			    handleDestroy(args);
			} else if (first.IndexOf(".iterator") != -1) {
			    handleIterator(args);
			} else if (first.IndexOf(".contains") != -1) {
			    handleContains(args);
			} else if (first.IndexOf(".stats") != -1) {
			    //handStats(args);
			} else if ("t.publish".Equals(first)) {
			    handleTopicPublish(args);
			} else if ("q.offer".Equals(first)) {
			    handleQOffer(args);
			} else if ("q.take".Equals(first)) {
			    handleQTake(args);
			} else if ("q.poll".Equals(first)) {
			    handleQPoll(args);
			} else if ("q.peek".Equals(first)) {
			    handleQPeek(args);
			} else if ("q.capacity".Equals(first)) {
			    handleQCapacity(args);
			} else if ("q.offermany".Equals(first)) {
			    handleQOfferMany(args);
			} else if ("q.pollmany".Equals(first)) {
			    handleQPollMany(args);
			} else if ("s.add".Equals(first)) {
			    handleSetAdd(args);
			} else if ("s.remove".Equals(first)) {
			    handleSetRemove(args);
			} else if ("s.addmany".Equals(first)) {
			    handleSetAddMany(args);
			} else if ("s.removemany".Equals(first)) {
			    handleSetRemoveMany(args);
			} else if (first.Equals("m.replace")) {
			    handleMapReplace(args);
			} else if (first.ToLower().Equals("m.putIfAbsent".ToLower())) {
			    handleMapPutIfAbsent(args);
			} else if (first.Equals("m.putAsync")) {
			    //handleMapPutAsync(args);
			} else if (first.Equals("m.getAsync")) {
			    //handleMapGetAsync(args);
			} else if (first.Equals("m.put")) {
			    handleMapPut(args);
			} else if (first.Equals("m.get")) {
			    handleMapGet(args);
			} else if (first.ToLower().Equals("m.getMapEntry".ToLower())) {
			    handleMapGetMapEntry(args);
			} else if (first.Equals("m.remove")) {
			    handleMapRemove(args);
			} else if (first.Equals("m.evict")) {
			    handleMapEvict(args);
			} else if (first.Equals("m.putmany") || first.ToLower().Equals("m.putAll".ToLower())) {
			    handleMapPutMany(args);
			} else if (first.Equals("m.getmany")) {
			    handleMapGetMany(args);
			} else if (first.Equals("m.removemany")) {
			    handleMapRemoveMany(args);
			} else if (command.ToLower().Equals("m.localKeys".ToLower())) {
			    //handleMapLocalKeys();
			} else if (command.Equals("m.keys")) {
			    handleMapKeys();
			} else if (command.Equals("m.values")) {
			    handleMapValues();
			} else if (command.Equals("m.entries")) {
			    handleMapEntries();
			} else if (first.Equals("m.lock")) {
			    handleMapLock(args);
			} else if (first.ToLower().Equals("m.tryLock".ToLower())) {
			    handleMapTryLock(args);
			} else if (first.Equals("m.unlock")) {
			    handleMapUnlock(args);
			} else if (first.IndexOf(".addListener") != -1) {
			    handleAddListener(args);
			} else if (first.Equals("m.removeMapListener")) {
			    handleRemoveListener(args);
			} else if (first.Equals("m.unlock")) {
			    handleMapUnlock(args);
			} else if (first.Equals("l.add")) {
			    handleListAdd(args);
			} else if ("l.addmany".Equals(first)) {
			    handleListAddMany(args);
			} else if (first.Equals("l.remove")) {
			    handleListRemove(args);
			} else if (first.Equals("l.contains")) {
			    handleListContains(args);
			} else if (first.Equals("execute")) {
			    //execute(args);
			} else if (first.Equals("partitions")) {
			    //handlePartitions(args);
			} else if (first.Equals("txn")) {
			    hazelcast.getTransaction().begin();
			} else if (first.Equals("commit")) {
			    hazelcast.getTransaction().commit();
			} else if (first.Equals("rollback")) {
			    hazelcast.getTransaction().rollback();
			} else if (first.ToLower().Equals("executeOnKey".ToLower())) {
			    //executeOnKey(args);
			} else if (first.ToLower().Equals("executeOnMember".ToLower())) {
			    //executeOnMember(args);
			} else if (first.ToLower().Equals("executeOnMembers".ToLower())) {
			    //executeOnMembers(args);
			} else if (first.ToLower().Equals("longOther".ToLower()) || first.ToLower().Equals("executeLongOther".ToLower())) {
			    //executeLongTaskOnOtherMember(args);
			} else if (first.ToLower().Equals("long") || first.ToLower().Equals("executeLong".ToLower())) {
			    //executeLong(args);
			} else if (first.ToLower().Equals("instances")) {
			    //handleInstances(args);
			} else if (first.ToLower().Equals("quit") || first.ToLower().Equals("exit")) {
			    return;
			} else {
			    println("type 'help' for help");
			}
		}
		protected void handleAddListener(String[] args) {
	        String first = args[0];
	        if (first.StartsWith("s.")) {
	            getSet().addItemListener(this, true);
	        } else if (first.StartsWith("m.")) {
	            if (args.Length > 1) {
	                getMap().addEntryListener(this, args[1], true);
	            } else {
	                getMap().addEntryListener(this, true);
	            }
	        } else if (first.StartsWith("q.")) {
	            getQueue().addItemListener(this, true);
	        } else if (first.StartsWith("t.")) {
	            getTopic().addMessageListener(this);
	        } else if (first.StartsWith("l.")) {
	            getList().addItemListener(this, true);
	        }
	    }
		
		protected void handleRemoveListener(String[] args) {
	        String first = args[0];
	        if (first.StartsWith("s.")) {
	            getSet().removeItemListener(this);
	        } else if (first.StartsWith("m.")) {
	            if (args.Length > 1) {
	                getMap().removeEntryListener(this, args[1]);
	            } else {
	                getMap().removeEntryListener(this);
	            }
	        } else if (first.StartsWith("q.")) {
	            getQueue().removeItemListener(this);
	        } else if (first.StartsWith("t.")) {
	            getTopic().removeMessageListener(this);
	        } else if (first.StartsWith("l.")) {
	            getList().removeItemListener(this);
	        }
	    }
		
		protected void handleSize(String[] args) {
	        int size = 0;
	        String iteratorStr = args[0];
	        if (iteratorStr.StartsWith("s.")) {
	            size = getSet().size();
	        } else if (iteratorStr.StartsWith("m.")) {
	            size = getMap().size();
	        } else if (iteratorStr.StartsWith("q.")) {
	            size = getQueue().Count;
	        } else if (iteratorStr.StartsWith("l.")) {
	            size = getList().size();
	        }
	        println("Size = " + size);
	    }
		
		protected void handleClear(String[] args) {
	        String iteratorStr = args[0];
	        if (iteratorStr.StartsWith("s.")) {
	            getSet().Clear();
	        } else if (iteratorStr.StartsWith("m.")) {
	            getMap().clear();
	        } else if (iteratorStr.StartsWith("q.")) {
	            getQueue().Clear();
	        } else if (iteratorStr.StartsWith("l.")) {
	            getList().Clear();
	        }
	        println("Cleared all.");
	    }
		
		protected void handleDestroy(String[] args) {
        	String iteratorStr = args[0];
	        if (iteratorStr.StartsWith("s.")) {
	            getSet().destroy();
	        } else if (iteratorStr.StartsWith("m.")) {
	            getMap().destroy();
	        } else if (iteratorStr.StartsWith("q.")) {
	            getQueue().destroy();
	        } else if (iteratorStr.StartsWith("l.")) {
	            getList().destroy();
	        }
	        println("Destroyed!");
	    }
		
		protected void handleContains(String[] args) {
	        String iteratorStr = args[0];
	        bool key = false;
	        bool value = false;
	        if (iteratorStr.ToLower().EndsWith("key")) {
	            key = true;
	        } else if (iteratorStr.ToLower().EndsWith("value")) {
	            value = true;
	        }
	        String data = args[1];
	        bool result = false;
	        if (iteratorStr.StartsWith("s.")) {
	            result = getSet().Contains(data);
	        } else if (iteratorStr.StartsWith("m.")) {
	            result = (key) ? getMap().containsKey(data) : getMap().containsValue(data);
	        } else if (iteratorStr.StartsWith("q.")) {
	            result = getQueue().Contains(data);
	        } else if (iteratorStr.StartsWith("l.")) {
	            result = getList().Contains(data);
	        }
	        println("Contains : " + result);
	    }
		
		protected void handleLock(String[] args) {
	        String lockStr = args[0];
	        String key = args[1];
	        ILock _lock = hazelcast.getLock(key);
	        if (lockStr.ToLower().Equals("lock")) {
				_lock.Lock();
	            println("true");
	        } else if (lockStr.ToLower().Equals("unlock")) {
	            _lock.unLock();
	            println("true");
	        } else if (lockStr.ToLower().Equals("trylock")) {
	            String timeout = args.Length > 2 ? args[2] : null;
	            if (timeout == null) {
	                println(_lock.tryLock());
	            } else {
	                long time = long.Parse(timeout);
	                println(_lock.tryLock(time*1000));
	                
	            }
	        }
	    }
		
		protected void handleIterator(String[] args) {
	        System.Collections.Generic.IEnumerator<object> it = null;
	        String iteratorStr = args[0];
	        if (iteratorStr.StartsWith("s.")) {
	            it = getSet().GetEnumerator();
	        } else if (iteratorStr.StartsWith("m.")) {
	            it = getMap().Keys().GetEnumerator();
	        } else if (iteratorStr.StartsWith("q.")) {
	            it = getQueue().GetEnumerator();
	        } else if (iteratorStr.StartsWith("l.")) {
	            it = getList().GetEnumerator();
	        }
	        
	        int count = 1;
	        while (it.MoveNext()) {
	            print(count++ + " " + it.Current);
	            println("");
	        }
	    }
		
		
		protected void handleSetAdd(String[] args) {
	        println(getSet().Add(args[1]));
	    }
		protected void handleSetRemove(String[] args) {
	        println(getSet().Remove(args[1]));
	    }
	
	    protected void handleSetAddMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        int successCount = 0;
	        System.DateTime t0 = System.DateTime.Now;
	        for (int i = 0; i < count; i++) {
	            bool success = getSet().Add("obj" + i);
	            if (success)
	                successCount++;
	        }
	        double seconds = (System.DateTime.Now - t0).TotalSeconds;
	        println("Added " + successCount + " objects.");
	        println("size = " + getSet().Count + ", " + successCount / seconds + " evt/s");
	    }
		
		 protected void handleListAddMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        int successCount = 0;
	        System.DateTime t0 = System.DateTime.Now;
	        for (int i = 0; i < count; i++) {
	            getList().Add("obj" + i);
	                successCount++;
	        }
	        double seconds = (System.DateTime.Now - t0).TotalSeconds;
	        println("Added " + successCount + " objects.");
	        println("size = " + getSet().Count + ", " + successCount / seconds + " evt/s");
	    }
	
	    protected void handleSetRemoveMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        int successCount = 0;
	        System.DateTime t0 = System.DateTime.Now;
	        for (int i = 0; i < count; i++) {
	            getSet().Remove("obj" + i);
	            successCount++;
	        }
	        double seconds = (System.DateTime.Now - t0).TotalSeconds;
	        println("Removed " + successCount + " objects.");
	        println("size = " + getSet().Count + ", " + successCount / seconds + " evt/s");
	    }
		
		protected void handleListAdd(String[] args) {
	        getList().Add(args[1]);
			println("true");
	        
	    }
		protected void handleListContains(String[] args) {
	        println(getList().Contains(args[1]));
	    }
	
	    protected void handleListRemove(String[] args) {
	        int index = -1;
	        index = int.Parse(args[1]);
	        if (index >= 0) {
				getList().RemoveAt(index);
	            println("true");
	        } else {
				getList().Remove(args[1]);
	            println("true");
	        }
	    }
		
		protected void handleTopicPublish(String[] args) {
	        getTopic().publish(args[1]);
	    }
		
		protected void handleQOffer(String[] args) {
	        long timeout = 0;
	        if (args.Length > 2) {
	            timeout = long.Parse(args[2]);
	        }
	        bool offered = getQueue().offer(args[1], timeout*1000);
	       	println(offered);
	    }
	
	    protected void handleQTake(String[] args) {
	     	println(getQueue().take());
	    }
	
	    protected void handleQPoll(String[] args) {
	        long timeout = 0;
	        if (args.Length > 1) {
	            timeout = long.Parse(args[1]);
	        }
	        
	        println(getQueue().poll(timeout*1000));
	        
	    }
	
	    protected void handleQOfferMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        Object value = null;
	        if (args.Length > 2)
	            value = new byte[int.Parse(args[2])];
	        DateTime t0 = System.DateTime.Now;
	        for (int i = 0; i < count; i++) {
	            if (value == null)
	                getQueue().offer("obj");
	            else
	                getQueue().offer(value);
	        }
	        DateTime t1 = System.DateTime.Now;
	        print("size = " + getQueue().Count + ", " + count * 1000 / (t1 - t0).TotalMilliseconds + " evt/s");
	        if (value == null) {
	            println("");
	        } else {
	            int b = int.Parse(args[2]);
	            println(", " + (count * 1000 / (t1 - t0).TotalMilliseconds) * (b * 8) / 1024 + " Kbit/s, "
	                    + count * b / 1024 + " KB added");
	        }
	    }
	
	    protected void handleQPollMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        int c = 1;
	        for (int i = 0; i < count; i++) {
	            Object obj = getQueue().poll();
	            if (obj is byte[]) {
	                println(c++ + " " + ((byte[]) obj).Length);
	            } else {
	                println(c++ + " " + obj);
	            }
	        }
	    }
		
		protected void handleMapTryLock(String[] args) {
	        String key = args[1];
	        long time = (args.Length > 2) ? long.Parse(args[2]) : 0;
	        bool locked = false;
	        if (time == 0)
	            locked = getMap().tryLock(key);
	        else
	            locked = getMap().tryLock(key, time*1000);
	        println(locked);
	    }
	
	    protected void handleMapUnlock(String[] args) {
	        getMap().unlock(args[1]);
	        println("true");
	    }
		
		protected void handleMapPutIfAbsent(String[] args) {
        	println(getMap().putIfAbsent(args[1], args[2]));
    	}
		
		protected void handleMapReplace(String[] args) {
	        println(getMap().replace(args[1], args[2]));
	    }
		
		protected void handleMapPut(String[] args) {
	        println(getMap().put(args[1], args[2]));
	    }
	
	    protected void handleMapGet(String[] args) {
	        println(getMap().get(args[1]));
	    }
	
	    protected void handleMapGetAsync(String[] args) {
	    	//println(getMap().getAsync(args[1]).get());
	    }
	
	    protected void handleMapGetMapEntry(String[] args) {
	        println(getMap().getMapEntry(args[1]));
	    }
	
	    protected void handleMapRemove(String[] args) {
	        println(getMap().remove(args[1]));
	    }
	
	    protected void handleMapEvict(String[] args) {
	        println(getMap().evict(args[1]));
	    }
	
	    protected void handleMapPutMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        int b = 100;
	        byte[] value = new byte[b];
	        if (args.Length > 2) {
	            b = int.Parse(args[2]);
	            value = new byte[b];
	        }
	        int start = getMap().size();
	        if (args.Length > 3) {
	            start = int.Parse(args[3]);
	        }
	        DateTime t0 = System.DateTime.Now;
			for (int i = 0; i < count; i++) {
	            getMap().put("key" + (start + i), value);
	        }
	        DateTime t1 = System.DateTime.Now;
	        if ((t1 - t0).TotalMilliseconds > 1) {
	            println("size = " + getMap().size() + ", " + count * 1000 / (t1 - t0).TotalMilliseconds
	                    + " evt/s, " + (count * 1000 / (t1 - t0).TotalMilliseconds) * (b * 8) / 1024 + " Kbit/s, "
	                    + count * b / 1024 + " KB added");
	        }
	    }
		
		protected void handleMapGetMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        for (int i = 0; i < count; i++) {
	            println(getMap().get("key" + i));
	        }
	    }
	
	    protected void handleMapRemoveMany(String[] args) {
	        int count = 1;
	        if (args.Length > 1)
	            count = int.Parse(args[1]);
	        int start = 0;
	        if (args.Length > 2)
	            start = int.Parse(args[2]);
	        DateTime t0 = System.DateTime.Now;
	        for (int i = 0; i < count; i++) {
	            getMap().remove("key" + (start + i));
	        }
	        DateTime t1 = System.DateTime.Now;
	        println("size = " + getMap().size() + ", " + count * 1000 / (t1 - t0).TotalMilliseconds + " evt/s");
	    }
		
		protected void handleMapKeys() {
	        System.Collections.Generic.ICollection<object> coll = getMap().Keys();
			int count = 0;
			foreach (object o in coll){
				count++;
	            println(o);
			}
	        println("Total " + count);	
	    }
	
	    protected void handleMapEntries() {
	        System.Collections.Generic.ICollection<object> coll = getMap().Keys();
			int count = 0;
			foreach (object o in coll){
				count++;
				MapEntry<object,object> entry = getMap().getMapEntry(o);
	            println(entry.getKey() + " : " + entry.getValue());
			}
	        println("Total " + count);
	    }
	
	    protected void handleMapValues() {
	        System.Collections.Generic.ICollection<object> coll = getMap().Keys();
			int count = 0;
			foreach (object o in coll){
				count++;
	            println(getMap().get(o));
			}
			println("Total " + count);	
	    }
	
	    protected void handleMapLock(String[] args) {
	        getMap().Lock(args[1]);
	        println("true");
		}		
		
		protected void handleQPeek(String[] args) {
	        println(getQueue().peek());
	    }
	
	    protected void handleQCapacity(String[] args) {
	        println(getQueue().remainingCapacity());
	    }
		
		
		
		
		
		public void onMessage<Object>(Message<Object> message){
			println("Topic received = " + message.getMessageObject());
		}
		public void entryAdded(EntryEvent<Object, Object> e){
			println(e.ToString());
		}
    	public void entryRemoved(EntryEvent<Object, Object> e){
			println(e.ToString());
		}
    	public void entryUpdated(EntryEvent<Object, Object> e){
			println(e.ToString());
		}
    
    	public void entryEvicted(EntryEvent<Object, Object> e){
			println(e.ToString());
		}
		public void itemAdded<Object>(ItemEvent<Object> item){
			println("Item added = " + item.Item);
		}
		public void itemRemoved<Object>(ItemEvent<Object> item){
			println("Item removed = " + item.Item);
		}
		
		protected void handleHelp(string command) {
	        bool silentBefore = silent;
	        silent = false;
	        println("Commands:");
	        println("-- General commands");
	        println("echo true|false                      //turns on/off echo of commands (default false)");
	        println("silent true|false                    //turns on/off silent of command output (default false)");
	        println("#<number> <command>                  //repeats <number> time <command>, replace $i in <command> with current iteration (0..<number-1>)");
	        println("&<number> <command>                  //forks <number> threads to execute <command>, replace $t in <command> with current thread number (0..<number-1>");
	        println("     When using #x or &x, is is advised to use silent true as well.");
	        println("     When using &x with m.putmany and m.removemany, each thread will get a different share of keys unless a start key index is specified");
	        println("jvm                                  //displays info about the runtime");
	        println("who                                  //displays info about the cluster");
	        println("whoami                               //displays info about this cluster member");
	        println("ns <string>                          //switch the namespace for using the distributed queue/map/set/list <string> (defaults to \"default\"");
	        println("@<file>                              //executes the given <file> script. Use '//' for comments in the script");
	        println("");
	        println("-- Queue commands");
	        println("q.offer <string>                     //adds a string object to the queue");
	        println("q.poll                               //takes an object from the queue");
	        println("q.offermany <number> [<size>]        //adds indicated number of string objects to the queue ('obj<i>' or byte[<size>]) ");
	        println("q.pollmany <number>                  //takes indicated number of objects from the queue");
	        println("q.iterator [remove]                  //iterates the queue, remove if specified");
	        println("q.size                               //size of the queue");
	        println("q.clear                              //clears the queue");
	        println("");
	        println("-- Set commands");
	        println("s.add <string>                       //adds a string object to the set");
	        println("s.remove <string>                    //removes the string object from the set");
	        println("s.addmany <number>                   //adds indicated number of string objects to the set ('obj<i>')");
	        println("s.removemany <number>                //takes indicated number of objects from the set");
	        println("s.iterator [remove]                  //iterates the set, removes if specified");
	        println("s.size                               //size of the set");
	        println("s.clear                              //clears the set");
	        println("");
	        println("-- Lock commands");
	        println("lock <key>                           //same as Hazelcast.getLock(key).lock()");
	        println("tryLock <key>                        //same as Hazelcast.getLock(key).tryLock()");
	        println("tryLock <key> <time>                 //same as tryLock <key> with timeout in seconds");
	        println("unlock <key>                         //same as Hazelcast.getLock(key).unlock()");
	        println("");
	        println("-- Map commands");
	        println("m.put <key> <value>                  //puts an entry to the map");
	        println("m.remove <key>                       //removes the entry of given key from the map");
	        println("m.get <key>                          //returns the value of given key from the map");
	        println("m.putmany <number> [<size>] [<index>]//puts indicated number of entries to the map ('key<i>':byte[<size>], <index>+(0..<number>)");
	        println("m.removemany <number> [<index>]      //removes indicated number of entries from the map ('key<i>', <index>+(0..<number>)");
	        println("     When using &x with m.putmany and m.removemany, each thread will get a different share of keys unless a start key <index> is specified");
	        println("m.keys                               //iterates the keys of the map");
	        println("m.values                             //iterates the values of the map");
	        println("m.entries                            //iterates the entries of the map");
	        println("m.iterator [remove]                  //iterates the keys of the map, remove if specified");
	        println("m.size                               //size of the map");
	        println("m.clear                              //clears the map");
	        println("m.destroy                            //destroys the map");
	        println("m.lock <key>                         //locks the key");
	        println("m.tryLock <key>                      //tries to lock the key and returns immediately");
	        println("m.tryLock <key> <time>               //tries to lock the key within given seconds");
	        println("m.unlock <key>                       //unlocks the key");
	        println("");
	        println("-- List commands:");
	        println("l.add <string>");
	        println("l.add <index> <string>");
	        println("l.contains <string>");
	        println("l.remove <string>");
	        println("l.remove <index>");
	        println("l.set <index> <string>");
	        println("l.iterator [remove]");
	        println("l.size");
	        println("l.clear");
	        println("execute	<echo-input>				//executes an echo task on random member");
	        println("execute0nKey	<echo-input> <key>		//executes an echo task on the member that owns the given key");
	        println("execute0nMember <echo-input> <key>	//executes an echo task on the member with given index");
	        println("execute0nMembers <echo-input> 		//executes an echo task on all of the members");
	        println("");
	        silent = silentBefore;
    	}
		
		public void print(Object o){
			Console.Write(o);
		}
		
		public void println(Object o){
			Console.WriteLine(o);
		}	
	}
}


