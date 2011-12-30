using System;


namespace Hazelcast.Core
{
	public class EntryEvent <K,V>
	{
		private K key;
    	private V oldValue;
    	private V value;
		private Member member;
		private String name;
		private bool collection;
		private EntryEventType entryEventType = EntryEventType.ADDED;

		public bool Collection {
			get {
				return this.collection;
			}
			set {
				collection = value;
			}
		}

		public EntryEventType EntryEventType {
			get {
				return this.entryEventType;
			}
			set {
				entryEventType = value;
			}
		}

		public K Key {
			get {
				return this.key;
			}
			set {
				key = value;
			}
		}

		public Member Member {
			get {
				return this.member;
			}
			set {
				member = value;
			}
		}

		public String Name {
			get {
				return this.name;
			}
			set {
				name = value;
			}
		}

		public V OldValue {
			get {
				return this.oldValue;
			}
			set {
				oldValue = value;
			}
		}

		public V Value {
			get {
				return this.value;
			}
			set {
				value = value;
			}
		}		
		public EntryEvent(String name, Member member, int eventType, K key, V oldValue, V value) {
        	this.name = name;
        	this.member = member;
        	this.key = key;
        	this.oldValue = oldValue;
        	this.value = value;
        	this.entryEventType =  (EntryEventType) EntryEventType.ToObject(typeof(EntryEventType), eventType);
    	}
		
		public override string ToString() 
  		{
			return "EntryEvent {" + Name
                + "} key=" + Key
                + ", oldValue=" + OldValue
                + ", value=" + Value
                + ", event=" + EntryEventType
                + ", by " + Member;
		}
		
	}
}

