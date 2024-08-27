insert into Tunnel_Actual(HaulingRequest_Reference, TunnelId, [Time], CreatedBy, CreatedOn)
select h.RecordIdx, t.TunnelId, Convert(varchar(10), BlendingDate, 120) + ' ' + case when (h.Shift = 1) then '07:00:00' else '19:00:00' end, 3, GetDate()
from HaulingRequest_Detail_PortionBlending h, Tunnel t 
where h.Tunnel = t.Name;
