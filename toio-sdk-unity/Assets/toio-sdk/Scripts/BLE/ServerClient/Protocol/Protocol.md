### S2C_WRITE
```
// head
- 0 : (U16)-buffer_size
- 2 : (byte)-S2C_WRITE
// body
- 3 : 台数
- for (i=2; 台数; ++) :
  - +0 : (byte)端末内ID
  - +1 : (byte)characteristicID
  - +2 : (byte)withResponse
  - +3 : (byte)buffsize
  - +4 : (byte[])bytes
```

### S2C_READ
```
// head
- 0 : (U16)-buffer_size
- 2 : (byte)-S2C_READ
// body
- 3 : (byte)端末内ID
- 4 : (byte)characteristicID
```

<br>

### C2S_CONNECT
```
// head
- 0 : (U16)-buffer_size
- 2 : (byte)-C2S_CONNECT
// body
- 3 : (U16)-listen_port
```

### C2S_JOIN
```
// head
- 0 : (U16)-buffer_size
- 2 : (byte)-C2S_JOIN
// body
- 3 : (byte)-端末内ID
- 4 : (byte)-addr_size
- 5 : (byte)-name_size
- 6 : (byte)-chara_count
- 7 : (byte[])-device_addr
- N : (byte[])-device_name
- for (; Characteristic.len; ++) :
  - +0 : (byte)-characteristicID
```

### C2S_JOINS
```
// head
- 0 : (U16)-buffer_size
- 2 : (byte)-C2S_JOINS
// body
- 3 : (byte)-台数
- for (; 台数; ++) :
  - +0 : (byte)-端末内ID
  - +1 : (byte)-addr_size
  - +2 : (byte)-name_size
  - +3 : (byte)-chara_size
  - +4 : (bytes[])-device_addr
  - +N : (bytes[])-device_name
  - for (; Characteristic.len; ++) :
    - ++0 : (byte)-characteristicID
```

### C2S_SUBSCRIBE
```
// head
- 0 : (U16)-buffer_size
- 2 : (byte)-C2S_SUBSCRIBE
// body
- 3 : (byte)-台数
- for (; 台数; ++) :
  - +0 : (byte)-端末内ID
  - +1 : (byte)-characteristicID
  - +2 : (byte)-buffsize
  - +3 : (byte[])-buff
```

### C2S_READ_CALLBACK
```
// head
- 0 : (U16)-buffer_size
- 2 : (byte)-C2S_READ_CALLBACK
// body
- 3 : (byte)端末内ID
- 4 : (byte)characteristicID
- 5 : (byte)-buffsize
- 6 : (byte[])-buff
```

