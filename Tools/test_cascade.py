import unittest, json
from dataclasses import replace, asdict
from collections import Counter
from cascade_model import *

def L(row='BACD',goal='ABCD',hp=2,**kw):return Level(tuple(row),tuple(goal),(Floor(hp,(True,)*(len(row)-1),**kw),))
class Tests(unittest.TestCase):
    def test_defuse_last_tick(self):
        l=L(bomb=Bomb(0,'A',1));s=step(l,initial(l),0)
        self.assertEqual((s.bomb_phase,s.defused,s.hp),('defused',1,1))
    def test_blast_preserves_row(self):
        l=L(hp=5,bomb=Bomb(3,'A',1));s=step(l,initial(l),0)
        self.assertEqual((s.row,s.hp,s.exploded),(tuple('ABCD'),2,1))
    def test_signed_repair(self):
        l=L(hp=1,fragile=(0,),repair=Sensor(0,'A'))
        self.assertEqual(step(l,initial(l),0).terminal,'won')
    def test_repair_saves_one_hp(self):
        l=L(hp=1,repair=Sensor(0,'A'));s=step(l,initial(l),0)
        self.assertEqual((s.hp,s.terminal),(1,''))
    def test_repair_one_shot(self):
        l=L(hp=6,repair=Sensor(0,'A'));s=initial(l)
        for a in [0,0,0]:s=step(l,s,a)
        self.assertEqual(s.hp,4)
    def test_repair_bomb_aggregate(self):
        l=L(hp=2,bomb=Bomb(3,'A',1),repair=Sensor(0,'A'))
        s=step(l,initial(l),0);self.assertEqual((s.terminal,s.exploded),('won',1))
    def test_overkill_does_not_carry(self):
        l=Level(tuple('BACD'),tuple('ABCD'),(Floor(1,(1,1,1),bomb=Bomb(3,'A',1,4)),Floor(3,(1,1,1))))
        self.assertEqual(step(l,initial(l),0).hp,3)
    def test_drop_ticks_old_bomb(self):
        l=L(bomb=Bomb(3,'A',1));s=step(l,initial(l),'D');self.assertEqual(s.exploded,1)
    def test_drop_abandons_not_defuses(self):
        l=L(bomb=Bomb(3,'A',2));s=step(l,initial(l),'D');self.assertEqual((s.abandoned,s.defused),(1,0))
    def test_drop_not_cancelled_by_repair(self):
        l=L('ABCD',hp=3,repair=Sensor(0,'A'));self.assertEqual(step(l,initial(l),'D').terminal,'won')
    def test_entry_contacts_after_transform(self):
        l=L('DABC',shift=-1,bomb=Bomb(0,'A'));s=initial(l)
        self.assertEqual((s.row,s.defused),(tuple('ABCD'),1))
    def test_entry_no_tick(self):
        l=L(bomb=Bomb(3,'A',2));self.assertEqual(initial(l).timer,2)
    def test_noop_no_tick(self):
        l=L('AABC','AABC',bomb=Bomb(3,'A',2));s=initial(l);self.assertEqual(step(l,s,0),s)
    def test_key_not_retroactive(self):
        l=Level(tuple('BACD'),tuple('ABCD'),(Floor(3,(False,1,1),key=Sensor(0,'A',0)),))
        s=initial(l);self.assertEqual(step(l,s,0),s)
    def test_key_then_exchange(self):
        l=Level(tuple('BCAD'),tuple('ABCD'),(Floor(2,(False,1,1),key=Sensor(1,'A',0)),))
        self.assertEqual(step(l,step(l,initial(l),1),0).terminal,'won')
    def test_linked_atomic(self):
        l=Level(tuple('BADC'),tuple('ABCD'),(Floor(1,(1,1,False),linked=(0,2)),))
        s=initial(l);self.assertEqual(step(l,s,0),s)
    def test_linked_same_pair_still_meaningful(self):
        l=L('AABA','AAAB',hp=1,linked=(0,2));self.assertEqual(step(l,initial(l),0).terminal,'won')
    def test_linked_fragile_additive(self):
        l=L('BADC',hp=3,linked=(0,2),fragile=(0,2));self.assertEqual(step(l,initial(l),0).terminal,'won')
    def test_linked_equivalent_commands(self):
        l=L('BADC',linked=(0,2));s=initial(l);self.assertEqual(step(l,s,0),step(l,s,2))
    def test_bridge_consumed(self):
        l=L(hp=3,bridge=0);s=step(l,initial(l),0);self.assertTrue(s.bridge_used);self.assertEqual(step(l,s,0),s)
    def test_invalid_bridge_move_does_not_consume(self):
        l=L('AABC','AABC',bridge=0);s=initial(l);self.assertEqual(step(l,s,0),s);self.assertFalse(s.bridge_used)
    def test_stamp_does_not_affect_win(self):
        l=L(stamp=Sensor(3,'A'));self.assertEqual(step(l,step(l,initial(l),0),'D').terminal,'won')
    def test_stamp_records_once(self):
        l=L(hp=5,stamp=Sensor(0,'A'));s=initial(l)
        for a in [0,0,0]:s=step(l,s,a)
        self.assertEqual(s.marks,1)
    def test_terminal_immutable(self):
        l=L(hp=1);s=step(l,initial(l),0);self.assertEqual(step(l,s,'D'),s)
    def test_cancel_invalid_returns_same(self):
        l=L();s=initial(l)
        for a in [-1,3,'bad',None,True]:self.assertEqual(step(l,s,a),s)
    def test_validate_multiset(self):
        with self.assertRaises(ValueError):initial(L('BACD','ABCE'))
    def test_validate_overlap(self):
        with self.assertRaises(ValueError):initial(L(linked=(0,1)))
    def test_validate_transform(self):
        with self.assertRaises(ValueError):initial(L(shift=1,mirror=True))
    def test_two_namespaces_not_equal(self):
        l=Level(('s:O','g:en:O','g:en:A'),('g:en:O','s:O','g:en:A'),(Floor(1,(1,1)),))
        self.assertEqual(step(l,initial(l),0).terminal,'won')
    def test_eight_tokens(self):
        l=L('BACDEFGH','ABCDEFGH',hp=1);self.assertEqual(step(l,initial(l),0).terminal,'won')
    def test_four_floors(self):
        l=Level(tuple('BACD'),tuple('ABCD'),tuple(Floor(1,(1,1,1)) for _ in range(4)));s=initial(l)
        for a in [0,'D','D','D']:s=step(l,s,a)
        self.assertEqual(s.terminal,'won')
    def test_snapshot_restore_replay(self):
        l=L('BADC',hp=1);s0=initial(l);s1=step(l,s0,0)
        self.assertEqual(s1,step(l,s0,0));self.assertEqual(s0.row,tuple('BADC'))
    def test_serialization_model(self):
        l=L(bomb=Bomb(0,'A'));self.assertEqual(from_dict(json.loads(json.dumps(asdict(l)))),l)
    def test_search_budget_is_unknown(self):
        with self.assertRaisesRegex(RuntimeError,'UNKNOWN'):graph(L('DCBA',hp=8),cap=1)
    def test_exact_graph_minimum(self):
        l=L('BCAD',hp=2);a=analyze(l);self.assertEqual((a['optimal_actions'],a['path']),(2,[1,0]))
    def test_unsolvable_not_unknown(self):
        a=analyze(L('DCBA',hp=1));self.assertFalse(a['solvable']);self.assertIsNone(a['optimal_actions'])
    def test_optional_stamp_route(self):
        l=L('ACBD',hp=5,stamp=Sensor(0,'C'));base=analyze(l);extra=analyze(l,lambda s:s.marks==1)
        self.assertEqual(base['optimal_actions'],2);self.assertEqual(extra['optimal_actions'],4)
    def test_unsolvable_optional_not_attached(self):
        l=L(hp=1,stamp=Sensor(3,'A'));self.assertFalse(analyze(l,lambda s:s.marks==1)['solvable'])
    def test_same_input_same_state(self):
        l=L('BADC',hp=2,linked=(0,2));s=initial(l)
        self.assertEqual(step(l,s,0),step(l,s,0))

if __name__=='__main__':unittest.main(verbosity=2)
